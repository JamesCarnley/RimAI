#!/bin/bash

# Configuration
# Users can override this by setting RIMWORLD_APP_DIR env var
# Default path to RimWorldMac.app
DEFAULT_APP_PATH="$HOME/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app"
RIMWORLD_APP_PATH="${RIMWORLD_APP_DIR:-$DEFAULT_APP_PATH}"

# Construct Executable Path
# Note: The executable name contains spaces on some Mac versions
EXEC_PATH="$RIMWORLD_APP_PATH/Contents/MacOS/RimWorld by Ludeon Studios"

# Check if executable exists
if [ ! -f "$EXEC_PATH" ]; then
    echo "Error: RimWorld executable not found at:"
    echo "$EXEC_PATH"
    echo "Please set RIMWORLD_APP_DIR to the folder containing RimWorldMac.app"
    exit 1
fi

# Check if we should skip build
SKIP_BUILD=false
for arg in "$@"; do
    if [ "$arg" == "--no-build" ]; then
        SKIP_BUILD=true
    fi
done

if [ "$SKIP_BUILD" = false ]; then
    echo "Building mod..."
    ./build.sh
    if [ $? -ne 0 ]; then
        echo "Build failed. Aborting launch."
        exit 1
    fi
else
    echo "Skipping build (--no-build specified)..."
fi

echo "Launching RimWorld in QuickTest mode..."
echo "Executable: $EXEC_PATH"

# Run with -quicktest
# -quicktest: Loads a test map immediately
# -logFile: Redirects log to a file for easier debugging (optional, but good practice)
"$EXEC_PATH" -quicktest -logFile /tmp/rimworld_quicktest.log &

echo "RimWorld launched! Logs at /tmp/rimworld_quicktest.log"
