#!/usr/bin/env bash
# Install git hooks for this repository
# Run this after cloning: bash tools/install-hooks.sh

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo "Configuring git hooks path to .githooks..."
git -C "$REPO_ROOT" config core.hooksPath .githooks

echo "Done. Pre-commit hook is now active."
echo "Backup files (.bak, .bak2, .orig, .swp) will be rejected on commit."
