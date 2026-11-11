# AI Review: Sample.FleetApi (feature/trip-average-speed)

_Add average speed to the trip summary_

- **Provider:** simulated (pattern rules, no model was called)
- **Change:** 3 files, +19 / -7 lines
- **Redacted before the prompt was built:** 1 × id-number, 1 × location, 1 × secret
- **Generated:** 2026-11-11 18:29 UTC

## Summary

- Stated intent: Add average speed to the trip summary
- Changes the public signature of `GetSummary`. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:16`)
- Adds `FindAsync`. (`src/Sample.FleetApi/Trips/TripRepository.cs:18`)
- Removes the public member `Find`. (`src/Sample.FleetApi/Trips/TripRepository.cs:18`)
- Changes the public signature of `TripSummary`. (`src/Sample.FleetApi/Trips/TripSummary.cs:3`)

## Suggestions

- **[MEDIUM]** Division by a value that may be zero. Guard the zero case and add a test for it. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:20`)  
  _correctness · confidence: low_
- **[MEDIUM]** No test file is part of this change. Add tests for the behaviour that changed.  
  _tests · confidence: high_
- **[LOW]** DateTime.Now depends on the server's time zone. Use DateTime.UtcNow or an injected TimeProvider. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:27`)  
  _correctness · confidence: high_
- **[LOW]** A TODO is being merged. Finish it or log a ticket and reference it here. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:23`)  
  _maintainability · confidence: high_

## Potential Risks

- **[HIGH]** Personal information is written into the source. Remove it and check how it got here. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:22`)  
  _privacy · confidence: medium_
- **[HIGH]** This log line writes identity or location data. Logs are kept and shared more widely than the database. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:24`)  
  _privacy · confidence: medium_
- **[HIGH]** A secret is hard-coded in this change. Move it to the secret store and rotate the value. (`src/Sample.FleetApi/Trips/TripRepository.cs:9`)  
  _security · confidence: high_
- **[HIGH]** SQL is built by joining strings. Use a parameterised query. (`src/Sample.FleetApi/Trips/TripRepository.cs:21`)  
  _security · confidence: medium_
- **[MEDIUM]** Existing callers of `GetSummary` may rely on the previous signature or behaviour. Check the callers and tell the teams that use it. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:16`)  
  _compatibility · confidence: medium_
- **[MEDIUM]** Blocking on an async call can deadlock and ties up a thread. Make the method async and await it. (`src/Sample.FleetApi/Trips/TripSummaryService.cs:18`)  
  _correctness · confidence: medium_
- **[MEDIUM]** Existing callers of `Find` may rely on the previous signature or behaviour. Check the callers and tell the teams that use it. (`src/Sample.FleetApi/Trips/TripRepository.cs:18`)  
  _compatibility · confidence: medium_
- **[MEDIUM]** Existing callers of `TripSummary` may rely on the previous signature or behaviour. Check the callers and tell the teams that use it. (`src/Sample.FleetApi/Trips/TripSummary.cs:3`)  
  _compatibility · confidence: medium_

---
This review is advice for a human reviewer. It does not approve or block the change.
