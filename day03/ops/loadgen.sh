#!/bin/sh
# Traffic giả cho dashboard: đoán số 1-100, FizzBuzz n=0..119 (n=0 hoặc >100 → 400).
rand() { echo $(( $(od -An -N1 -tu1 /dev/urandom) % $1 )); }

while true; do
  curl -s -o /dev/null -X POST http://weblab:8080/api/guess \
    -H 'Content-Type: application/json' -d "{\"number\":$(( $(rand 100) + 1 ))}"
  curl -s -o /dev/null "http://weblab:8080/api/fizzbuzz?n=$(rand 120)"
  sleep 0.2
done
