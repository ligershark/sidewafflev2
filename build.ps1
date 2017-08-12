[cmdletbinding()]
param()

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDir = ((Get-ScriptDirectory) + "\")

$global:buildsettings = New-Object -TypeName psobject -Property @{
    SourceRoot = ($scriptDir)
}

function InternalOverrideSettingsFromEnv{
    [cmdletbinding()]
    param(
        [Parameter(Position=0)]
        [object[]]$settings = ($global:PSBuildSettings),

        [Parameter(Position=1)]
        [string]$prefix = 'PSBuild'
    )
    process{
        foreach($settingsObj in $settings){
            if($settingsObj -eq $null){
                continue
            }

            $settingNames = $null
            if($settingsObj -is [hashtable]){
                $settingNames = $settingsObj.Keys
            }
            else{
                $settingNames = ($settingsObj | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name)

            }

            foreach($name in ($settingNames.Clone())){
                $fullname = ('{0}{1}' -f $prefix,$name)
                if(Test-Path "env:$fullname"){
                    'Updating setting [{0}] to [{1}]' -f ($settingsObj.$name),((get-childitem "env:$fullname").Value) | Write-Verbose
                    $settingsObj.$name = ((get-childitem "env:$fullname").Value)
                }
            }
        }
    }
}
InternalOverrideSettingsFromEnv -settings $global:buildsettings

<#
.SYNOPSIS
    You can add this to you build script to ensure that psbuild is available before calling
    Invoke-MSBuild. If psbuild is not available locally it will be downloaded automatically.
#>
function EnsurePsbuildInstlled{
    [cmdletbinding()]
    param(
        [string]$psbuildInstallUri = 'https://raw.githubusercontent.com/ligershark/psbuild/master/src/GetPSBuild.ps1'
    )
    process{
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            'Installing psbuild from [{0}]' -f $psbuildInstallUri | Write-Verbose
            (new-object Net.WebClient).DownloadString($psbuildInstallUri) | iex
        }
        else{
            'psbuild already loaded, skipping download' | Write-Verbose
        }

        # make sure it's loaded and throw if not
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            throw ('Unable to install/load psbuild from [{0}]' -f $psbuildInstallUri)
        }
    }
}

function EnsureDirectoryExists{
    [cmdletbinding()]
    param(
        [string]$path
    )
    process{
        if(-not (Test-Path $path)){
            New-Item -Path $path -ItemType Directory
        }
    }
}

function CopyFiles{
    [cmdletbinding()]
    param(
        $files
    )
    process{
        foreach($file in $files){
            $src = $file.Source
            $dest = $file.Dest

            EnsureDirectoryExists (Split-Path $dest -Parent)
            @'
Copy
    src:  {0}
    dest: {1}
'@ -f $src, $dest  | Write-Verbose
            Copy-Item -LiteralPath $src -Destination $dest
        }
    }
}

function CopyStaticFilesToOtherProjects{
    [cmdletbinding()]
    param(
        [string]$srcRoot = ($global:buildsettings.SourceRoot)
    )
    process{

        $filesToCopy = 
        <# The file is customized on purpose, don't overwrite it

        @{
            'Source'=(Join-Path $srcRoot 'templatepack.proj')
            'Dest' = (join-path $srcRoot 'SideWaffle.Creator\template\templatepack.proj')
        },#>
        @{
            'Source'=(Join-Path $srcRoot 'templatepack.proj')
            'Dest' = (join-path $srcRoot 'templates\SideWaffle.Template\template\templatepack.proj')
        },

        @{
            'Source'=(Join-Path $srcRoot 'wafflebuilder.targets')
            'Dest' = (join-path $srcRoot 'SideWaffle.Creator\Properties\wafflebuilder.targets')
        },
        @{
            'Source'=(Join-Path $srcRoot 'wafflebuilder.targets')
            'Dest' = (join-path $srcRoot 'templates\SideWaffle.Template\Properties\wafflebuilder.targets')
        }

        CopyFiles -files $filesToCopy
    }
}

function BuildAll{
    [cmdletbinding()]
    param()
    process{
        'Build started' | Write-Output
        CopyStaticFilesToOtherProjects
    }
}

# start script
BuildAll
