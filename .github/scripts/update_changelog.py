#!/usr/bin/env python3
"""Promote the ``[Unreleased]`` changelog section into a dated release section.

Moves everything under ``## [Unreleased]`` into a new ``## [VERSION] - DATE``
section, leaves a fresh empty ``[Unreleased]`` behind, and regenerates the
compare links at the bottom of the file (Keep a Changelog format).

Fails (non-zero exit) when there is nothing to release or the version section
already exists, so the release workflow stops before creating a tag.
"""
import argparse
import re
import sys

HEADING = re.compile(r"^##\s+\[([^\]]+)\]")
LINK_DEF = re.compile(r"^\[[^\]]+\]:\s+http")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", required=True, help="SemVer to release, e.g. 1.3.0")
    parser.add_argument("--date", required=True, help="Release date, YYYY-MM-DD")
    parser.add_argument("--repo", required=True, help="owner/name, e.g. emuuu/KASserver.NET")
    parser.add_argument("--package", required=True, help="Tag prefix, e.g. kasserver")
    parser.add_argument("--file", default="CHANGELOG.md")
    args = parser.parse_args()

    version = args.version.strip()

    with open(args.file, encoding="utf-8") as handle:
        lines = handle.read().splitlines()

    # Drop the existing link-reference block; it is regenerated deterministically below.
    content = [line for line in lines if not LINK_DEF.match(line)]

    # Locate the [Unreleased] heading and the next version heading after it.
    unreleased_idx = next(
        (i for i, line in enumerate(content)
         if (m := HEADING.match(line)) and m.group(1).lower() == "unreleased"),
        None,
    )
    if unreleased_idx is None:
        sys.exit("error: no '## [Unreleased]' heading found in changelog")

    next_idx = next(
        (i for i in range(unreleased_idx + 1, len(content)) if HEADING.match(content[i])),
        len(content),
    )

    # Extract the Unreleased entries, trimmed of surrounding blank lines.
    body = content[unreleased_idx + 1:next_idx]
    while body and not body[0].strip():
        body.pop(0)
    while body and not body[-1].strip():
        body.pop()

    if not body:
        sys.exit("error: no entries under [Unreleased]; add changelog entries before releasing a stable version")

    existing = [
        m.group(1)
        for line in content[next_idx:]
        if (m := HEADING.match(line)) and m.group(1).lower() != "unreleased"
    ]
    if version in existing:
        sys.exit(f"error: changelog already has a [{version}] section")

    before = content[:unreleased_idx + 1]   # ... up to and including '## [Unreleased]'
    after = content[next_idx:]              # existing version sections

    new_section = ["", f"## [{version}] - {args.date}", "", *body, ""]
    body_lines = before + new_section + after
    while body_lines and not body_lines[-1].strip():
        body_lines.pop()

    # Regenerate the compare links, newest first.
    versions = [version] + existing
    base = f"https://github.com/{args.repo}"
    pkg = args.package
    links = [f"[Unreleased]: {base}/compare/{pkg}/v{version}...HEAD"]
    for i, current in enumerate(versions):
        older = versions[i + 1] if i + 1 < len(versions) else None
        if older:
            links.append(f"[{current}]: {base}/compare/{pkg}/v{older}...{pkg}/v{current}")
        else:
            links.append(f"[{current}]: {base}/releases/tag/{pkg}/v{current}")

    output = "\n".join(body_lines) + "\n\n" + "\n".join(links) + "\n"
    with open(args.file, "w", encoding="utf-8") as handle:
        handle.write(output)

    print(f"Promoted [Unreleased] -> [{version}] - {args.date} ({len(body)} entry line(s))")


if __name__ == "__main__":
    main()
