# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.1] - 2023-10-29

### Added

- Added `VIEW` to Uploaded Count request.

## [2.0.0] - 2023-10-29

### Added

- Added the following attributes to the records table: `downloads_count`, `likes_count`, `rating_count`, `rating_avg`.
- Added `ADMIN_ENVIRONMENT` configuration instead of `TEST_ENVIRONMENT`. 
- Added the `USER_ID` request (only for `ADMIN_ENVIRONMENT`).
- Added `FORCE_UPDATE_ALL_COUNT` request (only for `ADMIN_ENVIRONMENT`) for complete update of `count` attributes in the records table.

### Changed

- Replace `YDB API` with `Document API` in `DOWNLOAD` and `LOAD_IMAGE` requests to reduce the cost of requests.
- Changed the `SELECT` query and reduced its cost by adding new count attributes.
- Changed Copyright in LICENSE.

## [1.0.0] - 2023-09-14

Initial release.