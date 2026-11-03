<#
.SYNOPSIS
    Compila Jurigest para Linux, lo sube al servidor y activa la nueva versión.

.EXAMPLE
    .\deploy\linux\publicar.ps1 -Servidor usuario@203.0.113.10

.EXAMPLE
    # Solo genera el paquete, sin subirlo.
    .\deploy\linux\publicar.ps1 -SoloEmpaquetar

.NOTES
    Requiere .NET SDK 10, dotnet-ef 10 y el cliente OpenSSH de Windows (ssh, scp).
    El servidor debe estar preparado con instalar-servidor.sh.
#>
[CmdletBinding()]
param(
    [string] $Servidor,
    [switch] $SoloEmpaquetar
)

$ErrorActionPreference = 'Stop'

function Paso([string] $texto) { Write-Host "`n==> $texto" -ForegroundColor Cyan }

function Ejecutar([string] $comando, [string[]] $argumentos) {
    & $comando @argumentos
    if ($LASTEXITCODE -ne 0) { throw "Falló: $comando $($argumentos -join ' ')" }
}

if (-not $SoloEmpaquetar -and -not $Servidor) {
    throw 'Indique -Servidor usuario@ip-o-dominio, o use -SoloEmpaquetar.'
}

$raiz = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $raiz

$cambios = git status --porcelain -- src
if ($cambios) {
    Write-Warning 'Hay cambios sin commit en src/. Se publicará el código tal como está en su equipo.'
}

$commit = (git rev-parse --short HEAD).Trim()
$version = "jurigest-$(Get-Date -Format 'yyyyMMdd-HHmmss')-$commit"
$trabajo = Join-Path $env:TEMP $version
$paquete = Join-Path $env:TEMP "$version.tar.gz"

Paso "Compilando la API para Linux ($version)"
Ejecutar dotnet @('publish', 'src/Jurigest.API/Jurigest.API.csproj', '-c', 'Release', '-r', 'linux-x64',
    '--self-contained', 'true', '-o', (Join-Path $trabajo 'api'))

Paso 'Compilando la Web para Linux'
Ejecutar dotnet @('publish', 'src/Jurigest.Web/Jurigest.Web.csproj', '-c', 'Release', '-r', 'linux-x64',
    '--self-contained', 'true', '-o', (Join-Path $trabajo 'web'))

Paso 'Generando el ejecutable de migraciones'
Ejecutar dotnet @('ef', 'migrations', 'bundle', '--project', 'src/Jurigest.Persistence',
    '--startup-project', 'src/Jurigest.API', '--configuration', 'Release', '--target-runtime', 'linux-x64',
    '--self-contained', '--force', '--output', (Join-Path $trabajo 'efbundle'))

# Los ajustes de desarrollo no deben llegar al servidor.
Get-ChildItem $trabajo -Recurse -Filter 'appsettings.Development.json' | Remove-Item

Paso 'Empaquetando'
if (Test-Path $paquete) { Remove-Item $paquete }
Ejecutar tar @('-czf', $paquete, '-C', $trabajo, '.')
Remove-Item $trabajo -Recurse -Force
$tamano = [math]::Round((Get-Item $paquete).Length / 1MB, 1)
Write-Host "Paquete: $paquete ($tamano MB)"

if ($SoloEmpaquetar) { return }

Paso "Subiendo a $Servidor"
Ejecutar scp @($paquete, "${Servidor}:/tmp/")

Paso 'Activando en el servidor'
Ejecutar ssh @('-t', $Servidor, "sudo jurigest-activar /tmp/$version.tar.gz")

Remove-Item $paquete
Write-Host "`nPublicación terminada." -ForegroundColor Green
