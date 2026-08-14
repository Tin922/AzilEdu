#!/usr/bin/env bash
set -euo pipefail
BASE="https://localhost:7205"
login() {
  curl -sk -X POST "$BASE/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$1\",\"password\":\"$2\"}" | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])"
}
status() {
  local method="$1" url="$2" token="${3:-}"
  if [[ -n "$token" ]]; then
    curl -sk -o /dev/null -w "%{http_code}" -X "$method" "$BASE$url" -H "Authorization: Bearer $token" "${@:4}"
  else
    curl -sk -o /dev/null -w "%{http_code}" -X "$method" "$BASE$url" "${@:4}"
  fi
}

ADMIN=$(login admin@aziledu.local 'Admin123!')
EMP=$(login employee@aziledu.local 'Employee123!')
VOL=$(login volunteer@aziledu.local 'Volunteer123!')
DON=$(login donor@aziledu.local 'Donor123!')

echo "=== AUTH TESTS ==="
printf "GET /api/animals | no token | expect 401 | got %s\n" "$(status GET /api/animals)"
printf "GET /api/animals | employee | expect 200 | got %s\n" "$(status GET /api/animals "$EMP")"
printf "POST /api/animals | donor | expect 403 | got %s\n" "$(status POST /api/animals "$DON" -H 'Content-Type: application/json' -d '{}')"
printf "GET /api/donations | donor | expect 403 | got %s\n" "$(status GET /api/donations "$DON")"
printf "GET /api/donations/mine | donor | expect 200 | got %s\n" "$(status GET /api/donations/mine "$DON")"
printf "GET /api/volunteertasks/mine | volunteer | expect 200 | got %s\n" "$(status GET /api/volunteertasks/mine "$VOL")"
printf "GET /api/volunteertasks/mine | donor | expect 403 | got %s\n" "$(status GET /api/volunteertasks/mine "$DON")"
printf "GET /api/users | employee | expect 403 | got %s\n" "$(status GET /api/users "$EMP")"
printf "GET /api/users | admin | expect 200 | got %s\n" "$(status GET /api/users "$ADMIN")"

echo "=== CREATE USER (Employee + Donor, linked to employee id 2) ==="
CREATE_HTTP=$(curl -sk -o /tmp/aziledu-create-user.json -w "%{http_code}" -X POST "$BASE/api/users" \
  -H "Authorization: Bearer $ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"email":"test.korisnik@aziledu.local","displayName":"Test Korisnik","password":"Test123!","isActive":true,"roleIds":[1,3,5],"employeeId":2,"donorId":2}')
CREATE_BODY=$(cat /tmp/aziledu-create-user.json)
printf "POST /api/users | admin | got %s\n" "$CREATE_HTTP"
if [[ "$CREATE_HTTP" == "201" ]]; then
  echo "$CREATE_BODY" | python3 -m json.tool
elif [[ "$CREATE_HTTP" == "400" ]]; then
  echo "User already exists or validation failed (OK for re-run): $CREATE_BODY"
else
  echo "$CREATE_BODY"
fi

echo "=== DONOR ISOLATION ==="
DONOR_DONATIONS=$(curl -sk "$BASE/api/donations/mine" -H "Authorization: Bearer $DON")
echo "$DONOR_DONATIONS" | python3 -c "
import sys, json
d = json.load(sys.stdin)
print('donor sees', len(d), 'donations')
ids = {(x.get('DonorId') or x.get('donorId')) for x in d}
print('donor ids:', ids)
"

echo "=== VOLUNTEER ISOLATION ==="
VOL_TASKS=$(curl -sk "$BASE/api/volunteertasks/mine" -H "Authorization: Bearer $VOL")
STAFF_TASKS=$(curl -sk "$BASE/api/volunteertasks?volunteerId=1" -H "Authorization: Bearer $EMP")
echo "$VOL_TASKS" | python3 -c "import sys,json; d=json.load(sys.stdin); print('volunteer mine:', len(d), 'tasks')"
echo "$STAFF_TASKS" | python3 -c "import sys,json; d=json.load(sys.stdin); print('staff filter vol1:', len(d), 'tasks')"

echo "=== MEDIA UPLOAD ==="
# tiny 1x1 png
echo 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==' | base64 -d > /tmp/test-upload.png
IMG=$(curl -sk -X POST "$BASE/api/animals/1/media" \
  -H "Authorization: Bearer $EMP" \
  -F "file=@/tmp/test-upload.png;type=image/png" \
  -F "caption=Test slika")
IMG_ID=$(echo "$IMG" | python3 -c "import sys,json; x=json.load(sys.stdin); print(x.get('id') or x.get('Id'))")
echo "uploaded image id=$IMG_ID"

# minimal mp4 header bytes won't work - use webm or create tiny file; try copying from app images if exists
APP_IMG=$(find "/run/media/tin/74CA1ED3CA1E918A/AzilEdu6/AzilEdu-Lekcija09-polazno" -name "*.webp" | head -1)
if [[ -n "$APP_IMG" ]]; then
  cp "$APP_IMG" /tmp/test-upload2.webp
  curl -sk -X POST "$BASE/api/animals/1/media" \
    -H "Authorization: Bearer $EMP" \
    -F "file=@/tmp/test-upload2.webp;type=image/webp" \
    -F "caption=Druga slika" > /dev/null
fi

COVER=$(curl -sk -o /dev/null -w "%{http_code}" -X PUT "$BASE/api/animals/1/media/$IMG_ID/cover" -H "Authorization: Bearer $EMP")
echo "set cover status=$COVER"

echo "=== AI ENDPOINTS (Mock) ==="
printf "GET /api/ai/daily-summary | employee | got %s\n" "$(status GET /api/ai/daily-summary "$EMP")"
printf "POST /api/ai/text | employee | got %s\n" "$(status POST /api/ai/text "$EMP" -H 'Content-Type: application/json' -d '{"purpose":"animal-adoption","input":"Luna, pas, mirna."}')"
printf "POST /api/ai/animal-intake | employee | got %s\n" "$(status POST /api/ai/animal-intake "$EMP" -H 'Content-Type: application/json' -d '{"text":"Pronađena mala mačka, oko 1 godine."}')"
printf "GET /api/ai/volunteer-summary/mine | volunteer | got %s\n" "$(status GET /api/ai/volunteer-summary/mine "$VOL")"

echo "=== POST ANIMAL (201) ==="
printf "POST /api/animals | employee | got %s\n" "$(status POST /api/animals "$EMP" -H 'Content-Type: application/json' -d '{"name":"Test","species":"Mačka","breed":"Domaća","gender":"Ženka","animalStatusId":1,"imageUrl":"/images/animals/placeholder.svg","description":"Test"}')"

echo "DONE"
