#!/usr/bin/env bash

API=http://localhost:8080/engine-rest
USERNAME=demo
PASSWORD=demo
PROCESS=Resources/process.bpmn

# curl -w "\n" --cookie-jar cookie.txt \
#   -H "Accept: application/json" \
#   -d "username=$USERNAME" \
#   -d "password=$PASSWORD" \
#   http://localhost:8080/camunda/api/admin/auth/user/default/login/cockpit

curl -w "\n" --cookie cookie.txt \
  -H "Accept: application/json" \
  -F "deployment-name=Visa Application" \
  -F "enable-duplicate-filtering=false" \
  -F "deploy-changed-only=false" \
  -F "process.bpmn=@$PROCESS" \
  -F "is_eligible_for_vwp.form=@resources/Forms/is_eligible_for_vwp.form" \
  -F "esta_form.form=@resources/Forms/esta_form.form" \
  -F "epassport.form=@resources/Forms/epassport.form" \
  -F "need_for_visa.form=@resources/Forms/need_for_visa.form" \
  -F "visa_application.form=@resources/Forms/visa_application.form" \
  -F "payment_information.form=@resources/Forms/payment_information.form" \
  $API/engine/default/deployment/create