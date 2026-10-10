#!/bin/bash

CHANNEL=${1:-development}
MESSAGE=${2:-"Atualizacao automatica gerada via Orca IDE"}

echo "Iniciando envio de atualizacao OTA (Over-The-Air) para o Z-Bank..."
echo "Canal: $CHANNEL"
echo "Mensagem: $MESSAGE"
echo "--------------------------------------------------------"

cd mobile || exit
npx eas-cli update --channel "$CHANNEL" --message "$MESSAGE" --auto
cd ..

echo "--------------------------------------------------------"
echo "Concluido! O app baixara as novidades ao ser reaberto no celular."
