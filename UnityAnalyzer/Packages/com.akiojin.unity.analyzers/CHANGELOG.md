# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.0 (2025-11-27)


### Bug Fixes

* rename .release-please-config.json to release-please-config.json ([d6086dd](https://github.com/akiojin/unity-analyzers/commit/d6086dd0a0b58e5829ae840652e773b9989d6859))

## [0.1.0] - 2025-11-26

### Added

- Initial release
- SR0001: Detect GetComponent calls in Update/FixedUpdate/LateUpdate loops
- SR0002: Detect forbidden Find methods (FindObjectOfType, GameObject.Find, etc.)
- SR0003: Detect null guards after GetComponent (Fail-Fast violation)
- SR0004: Detect null guards after DI injection (Fail-Fast violation)
