#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_FILE="$ROOT_DIR/src/ado-toolkit.csproj"
DIST_DIR="${DIST_DIR:-$ROOT_DIR/dist/homebrew}"
OWNER="${GITHUB_OWNER:-JohnnyDevCraft}"
REPO="${GITHUB_REPO:-ado-toolkit}"
VERSION="${VERSION:-$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$PROJECT_FILE" | head -n 1)}"
TAG="${TAG:-v$VERSION}"
HOMEPAGE="${HOMEPAGE:-https://github.com/$OWNER/$REPO}"
RELEASE_BASE_URL="${RELEASE_BASE_URL:-$HOMEPAGE/releases/download/$TAG}"

usage() {
  cat <<EOF
Usage: $(basename "$0") [--skip-publish]

Build Homebrew release tarballs for macOS, compute checksums, and generate a
tap-ready formula.

Environment overrides:
  VERSION            Release version. Default: from src/ado-toolkit.csproj
  TAG                Release tag. Default: v<VERSION>
  GITHUB_OWNER       GitHub owner. Default: JohnnyDevCraft
  GITHUB_REPO        Repository name. Default: ado-toolkit
  DIST_DIR           Output directory. Default: dist/homebrew
  RELEASE_BASE_URL   Base URL for release assets
EOF
}

require_file() {
  local path="$1"
  local message="$2"
  if [[ ! -f "$path" ]]; then
    echo "$message" >&2
    exit 1
  fi
}

SKIP_PUBLISH="false"
if [[ "${1:-}" == "--help" || "${1:-}" == "-h" ]]; then
  usage
  exit 0
fi

if [[ "${1:-}" == "--skip-publish" ]]; then
  SKIP_PUBLISH="true"
fi

mkdir -p "$DIST_DIR"

declare -A RID_MAP=(
  ["macos-arm64"]="osx-arm64"
  ["macos-x64"]="osx-x64"
)

for target in "${!RID_MAP[@]}"; do
  rid="${RID_MAP[$target]}"
  publish_dir="$DIST_DIR/publish/$target"
  archive_root="$DIST_DIR/staging/$target/ado-toolkit"

  rm -rf "$publish_dir" "$archive_root"
  mkdir -p "$publish_dir" "$archive_root"

  if [[ "$SKIP_PUBLISH" != "true" ]]; then
    dotnet publish "$PROJECT_FILE" \
      --configuration Release \
      --runtime "$rid" \
      --framework net10.0 \
      --output "$publish_dir" \
      -p:PublishSingleFile=true \
      -p:SelfContained=true \
      -p:PublishTrimmed=false
  fi

  require_file "$publish_dir/ado" "Expected packaged executable at $publish_dir/ado. Run without --skip-publish or provide staged publish outputs."

  cp "$publish_dir/ado" "$archive_root/ado"
  cp "$ROOT_DIR/README.md" "$archive_root/README.md"

  archive_path="$DIST_DIR/ado-toolkit-${VERSION}-${target}.tar.gz"
  tar -C "$DIST_DIR/staging/$target" -czf "$archive_path" "ado-toolkit"
done

arm_sha="$(shasum -a 256 "$DIST_DIR/ado-toolkit-${VERSION}-macos-arm64.tar.gz" | awk '{print $1}')"
x64_sha="$(shasum -a 256 "$DIST_DIR/ado-toolkit-${VERSION}-macos-x64.tar.gz" | awk '{print $1}')"

formula_path="$DIST_DIR/ado-toolkit.rb"
template_path="$ROOT_DIR/packaging/homebrew/ado-toolkit.rb.template"
description="Bridge Azure DevOps context into developer and AI workflows"
require_file "$template_path" "Expected Homebrew formula template at $template_path."

sed \
  -e "s|__DESC__|$description|g" \
  -e "s|__HOMEPAGE__|$HOMEPAGE|g" \
  -e "s|__VERSION__|$VERSION|g" \
  -e "s|__RELEASE_BASE_URL__|$RELEASE_BASE_URL|g" \
  -e "s|__ARM64_SHA__|$arm_sha|g" \
  -e "s|__X64_SHA__|$x64_sha|g" \
  "$template_path" > "$formula_path"

cat <<EOF
Created release artifacts in $DIST_DIR
  - ado-toolkit-${VERSION}-macos-arm64.tar.gz
  - ado-toolkit-${VERSION}-macos-x64.tar.gz
  - ado-toolkit.rb

Checksums
  macos-arm64: $arm_sha
  macos-x64:   $x64_sha
EOF
