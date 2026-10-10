param (
    [string]$Channel = "development",
    [string]$Message = "Atualizacao automatica gerada via Orca IDE"
)

Write-Host "Iniciando envio de atualizacao OTA (Over-The-Air) para o Z-Bank..."
Write-Host "Canal: $Channel"
Write-Host "Mensagem: $Message"
Write-Host "--------------------------------------------------------"

cd mobile

# Verifica se o EAS CLI está instalado, senão usa npx
npx eas-cli update --channel $Channel --message "$Message" --auto

cd ..
Write-Host "--------------------------------------------------------"
Write-Host "Concluido! O app baixara as novidades ao ser reaberto no celular."
