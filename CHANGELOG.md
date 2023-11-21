# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added some `SortParameters` for the `SELECT` query. Now the sorting of the `SELECT` query is tightly constrained.

### Changed

- Changed the structure of the `SELECT` query. Now `SELECT` query is split into two: `SELECT` and `SELECT_SELF`.
- The `rating_avg` attribute has been changed from `float` type to `Uint32`.
- Split `EntryType` into `CountEntryType` and `SelectEntryType`.
- Partially rewritten some of the tests.
- `RatingAverage` type changed to `Uint32`.
- The `COUNT` request returns only the number of all records.

### Fixed

- The `UPLOAD` query sets the metric attributes to 0 instead of null.

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