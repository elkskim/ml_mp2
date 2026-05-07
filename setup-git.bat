@echo off
REM Git Setup Script for ML_MP2 Project

echo.
echo ============================================
echo Setting up Git Repository
echo ============================================
echo.

cd /d "C:\Users\elksk\Documents\rimeligt legit skolesager\Programmeringsprojekter\MaL\ML_MP2"

REM Check if git is initialized
if exist .git (
    echo Repository already initialized
) else (
    echo Initializing git repository...
    git init
)

echo.
echo Configuring git user...
git config user.name "elkskim"
git config user.email "your-email@example.com"

echo.
echo Adding remote origin...
git remote remove origin 2>nul
git remote add origin https://github.com/elkskim/ml_mp2.git

echo.
echo Current git status:
git status

echo.
echo ============================================
echo Setup Complete!
echo ============================================
echo.
echo Next steps:
echo 1. Run: git add .
echo 2. Run: git commit -m "Initial commit: Research Scout Agent"
echo 3. Run: git push -u origin main
echo.
pause

