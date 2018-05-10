#!/usr/bin/env bash

key='rsa/private.key'
cert='rsa/cert.pem'
pfx='pfx/cert.pfx'
password='testPassword'

openssl req -x509 \
    -days 1000 \
    -nodes \
    -newkey rsa:2048 \
    -keyout $key \
    -out $cert

openssl pkcs12 -export \
    -in $cert \
    -inkey $key \
    -out $pfx \
    -password pass:$password
