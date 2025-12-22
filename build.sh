#!/bin/bash

# ==========================================
# RimAI Build & Install Script
# ==========================================

# 1. Configuration & Setup
# ------------------------
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LIBS_DIR="$PROJECT_ROOT/Libs"
SOURCE_DIR="$PROJECT_ROOT/Source"
OUTPUT_DIR="$PROJECT_ROOT/1.5/Assemblies"
RIMWORLD_MANAGED_PATH="$1"

# Target Directory for Installation
# Users can override this by setting RIMWORLD_MODS_DIR env var
TARGET_MODS_DIR="${RIMWORLD_MODS_DIR:-/Users/james/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Mods}"
MOD_NAME="RimAI"
DEST_DIR="$TARGET_MODS_DIR/$MOD_NAME"

# Check dependencies
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK (command 'dotnet') could not be found."
    echo "Please install the .NET 8 SDK (or newer)."
    exit 1
fi

# Find RimWorld Managed Data if not provided
if [ -z "$RIMWORLD_MANAGED_PATH" ]; then
    DEFAULT_PATH="$HOME/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed"
    if [ -d "$DEFAULT_PATH" ]; then
        echo "Found RimWorld Managed Data at default location."
        RIMWORLD_MANAGED_PATH="$DEFAULT_PATH"
    else
        echo "Usage: ./build.sh <PathToRimWorldManagedData>"
        exit 1
    fi
fi

# 2. Build Process
# ----------------
echo "--- Starting Build ---"

# Setup Libs
mkdir -p "$LIBS_DIR"
if [ ! -f "$LIBS_DIR/Assembly-CSharp.dll" ]; then
    echo "Copying references..."
    cp "$RIMWORLD_MANAGED_PATH/Assembly-CSharp.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.CoreModule.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.IMGUIModule.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.TextRenderingModule.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.UnityWebRequestModule.dll" "$LIBS_DIR/"
    cp "$RIMWORLD_MANAGED_PATH/System.Net.Http.dll" "$LIBS_DIR/"
fi

# Ensure UnityWebRequestModule is present (added later)
if [ ! -f "$LIBS_DIR/UnityEngine.UnityWebRequestModule.dll" ]; then
    echo "Copying UnityWebRequestModule..."
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.UnityWebRequestModule.dll" "$LIBS_DIR/"
fi

if [ ! -f "$LIBS_DIR/UnityEngine.UnityWebRequestWWWModule.dll" ]; then
    echo "Copying UnityWebRequestWWWModule..."
    cp "$RIMWORLD_MANAGED_PATH/UnityEngine.UnityWebRequestWWWModule.dll" "$LIBS_DIR/"
fi

# Restore/Add Packages
echo "Restoring packages..."
dotnet restore "$SOURCE_DIR/RimAI.csproj"

# Build
echo "Building..."
dotnet build "$SOURCE_DIR/RimAI.csproj" -c Debug

# Copy to local 1.6/Assemblies (1.5 support removed)
echo "Updating local assemblies for 1.6..."
OUTPUT_DIR_16="$PROJECT_ROOT/1.6/Assemblies"
mkdir -p "$OUTPUT_DIR_16"

# Copy all compiled DLLs (including dependencies like Newtonsoft.Json)
cp "$SOURCE_DIR/bin/Debug/"*.dll "$OUTPUT_DIR_16/"

# 3. Installation
# ---------------
echo "--- Installing Mod ---"
if [ ! -d "$TARGET_MODS_DIR" ]; then
    echo "Error: Mods directory not found at $TARGET_MODS_DIR"
    exit 1
fi

echo "Installing to: $DEST_DIR"

# Clean
if [ -d "$DEST_DIR" ]; then
    rm -rf "$DEST_DIR"
fi
mkdir -p "$DEST_DIR"

# Copy Files
echo "Copying files..."
cp -r "$PROJECT_ROOT/About" "$DEST_DIR/"
cp -r "$PROJECT_ROOT/Defs" "$DEST_DIR/"
# Only copy 1.6
cp -r "$PROJECT_ROOT/1.6" "$DEST_DIR/"
if [ -d "$PROJECT_ROOT/Textures" ]; then
    cp -r "$PROJECT_ROOT/Textures" "$DEST_DIR/"
fi
if [ -f "$PROJECT_ROOT/LoadFolders.xml" ]; then
    cp "$PROJECT_ROOT/LoadFolders.xml" "$DEST_DIR/"
fi

echo "----------------------------------------"
echo "Build & Install Complete!"
echo "✅ Mod installed to $DEST_DIR"
