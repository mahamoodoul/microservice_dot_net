path "transit/encrypt/my-encryption-key" {
  capabilities = ["update"]
}

path "transit/decrypt/my-encryption-key" {
  capabilities = ["update"]
}

path "transit/rotate/my-encryption-key" {
  capabilities = ["update"]
}
