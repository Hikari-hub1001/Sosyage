@echo off
setlocal

REM ====== Unity.exe のパス（ここを自分の環境に合わせて変更）======
set "UNITY_EXE=D:\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"

REM 1個目の引数があれば Unity.exe のパスとして上書き
if not "%~1"=="" set "UNITY_EXE=%~1"

REM ====== プロジェクトパス（このbatがあるフォルダをプロジェクト直下にする想定）======
set "PROJECT_PATH=%~dp0Sosyage"
REM 末尾の \ を消したい場合があるので一応整形
if "%PROJECT_PATH:~-1%"=="\" set "PROJECT_PATH=%PROJECT_PATH:~0,-1%"

REM ====== ログ出力先 ======
set "LOG_PATH=%~dp0Logs\AddressablesBuild.log"
if not exist "%~dp0Logs" mkdir "%~dp0\Logs"

REM ====== 実行する Editor メソッド（後述のC#と合わせる）======
set "EXEC_METHOD=BuildTools.AddressablesBuilder.BuildAddressables"

echo.
echo [Addressables] Unity: "%UNITY_EXE%"
echo [Addressables] Project: "%PROJECT_PATH%"
echo [Addressables] Log: "%LOG_PATH%"
echo [Addressables] Method: %EXEC_METHOD%
echo.

REM ビルド前に消したいフォルダ（プロジェクト直下からの相対パス）
set "CLEAN_DIR=ServerData"

echo 先にフォルダ削除
set "TARGET=%PROJECT_PATH%\%CLEAN_DIR%"
REM if exist "%TARGET%" (
REM  echo [PreClean] Delete "%TARGET%"
REM  rmdir /s /q "%TARGET%"
REM ) else (
REM  echo [PreClean] Skip (not found) "%TARGET%"
REM )

echo ====== Unity バッチ実行 ======
"%UNITY_EXE%" ^
  -batchmode ^
  -nographics ^
  -quit ^
  -projectPath "%PROJECT_PATH%" ^
  -executeMethod %EXEC_METHOD% ^
  -logFile "%LOG_PATH%"

set "EXITCODE=%ERRORLEVEL%"

echo.
if %EXITCODE% NEQ 0 (
  echo [Addressables] FAILED. exitcode=%EXITCODE%
  echo Log: "%LOG_PATH%"
) else (
  echo [Addressables] SUCCESS.
  echo Log: "%LOG_PATH%"
)

pause

exit /b %EXITCODE%
