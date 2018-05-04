#!/usr/bin/env bash

key='private.key'
cert='cert.pem'
pfx='cert.pfx'
password='testPassword'

openssl req -x509 \
    -days 1 \
    -nodes \
    -newkey rsa:2048 \
    -keyout $key \
    -out $cert

openssl pkcs12 -export \
    -in $cert \
    -inkey $key \
    -out $pfx \
    -password pass:$password
