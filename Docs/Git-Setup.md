# Git Setup

One-time setup each dev runs once per clone. Everything else (`.gitattributes`, `.gitignore`)
is already in the repo and needs no action.

## Line endings — nothing to do

`.gitattributes` pins this: everything is stored as LF in the repository, C# and text files are
checked out with your platform's native endings, and Unity's YAML assets (`.unity`, `.prefab`,
`.asset`, `.meta`, …) stay LF everywhere because that is what Unity itself writes.

Binary assets (`.png`, `.aseprite`, `.wav`, `.fbx`, …) are marked binary so git never tries to
diff or convert them.

## Unity SmartMerge (recommended)

Unity ships **UnityYAMLMerge**, a merge tool that understands scene and prefab YAML. Without it,
git merges those files line by line and usually produces a corrupt scene. `.gitattributes` already
requests it (`merge=unityyamlmerge`); this connects it to your machine.

Run once, from the repo root:

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.5.0f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force --fallback none %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

Adjust the path if your Unity install differs — on macOS it is
`/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/Tools/UnityYAMLMerge`.

What the flags do: `-h` headless (never opens a dialog mid-merge), `-p` premerge (resolve
everything it safely can), `--force` merge regardless of file extension, `--fallback none` don't
launch an external diff tool — if something can't be resolved, git reports a normal conflict.

Verify it took:

```bash
git config --get merge.unityyamlmerge.driver
```

Skipping this is not fatal — git falls back to its default text merge, exactly as before.

## This is a safety net, not the plan

The rule that actually prevents scene conflicts stays: **only one person edits a given scene at a
time**, build content as prefabs, add them to the scene once, keep scenes thin. SmartMerge is what
catches the mistake, not permission to make it.
