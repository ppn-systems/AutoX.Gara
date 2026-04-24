# Skill Benchmark: maui-collectionview — Iteration 2

**Model**: claude-sonnet-4-6
**Date**: 2026-03-26
**Evals**: 1, 2, 3 (1 run each per configuration)
**Context**: Testing leaner refactored SKILL.md (92 lines; reference content moved to references/)

## Summary

| Metric | With Skill | Without Skill | Delta |
|--------|------------|---------------|-------|
| Pass Rate | 100% ± 0% | 96.3% ± 5.2% | +3.7pp |
| Time | 85.1s ± 10.1s | 98.4s ± 1.5s | −13.3s |
| Tokens | 19,396 ± 541 | 16,061 ± 153 | +3,335 |

## Comparison with Iteration 1

| Metric | Iter 1 (With Skill) | Iter 2 (With Skill) |
|--------|---------------------|---------------------|
| Pass Rate | 100% | 100% ✅ |
| Time | 115.8s | 85.1s (↑ 30s faster) |
| Tokens | ~2,483 | 19,396 (↑ more — now reads full skill + references) |

The refactoring maintained 100% pass rate. Faster execution in iteration 2 is likely due to model/context improvements.

## Per-Eval Results

| Eval | With Skill | Without Skill |
|------|-----------|---------------|
| Recipe Grid (9 assertions) | 9/9 (100%) | 8/9 (89%) |
| Grouped Catalog (7 assertions) | 7/7 (100%) | 7/7 (100%) |
| Chat Scroll (5 assertions) | 5/5 (100%) | 5/5 (100%) |

## Key Finding

The only assertion that discriminates between with/without skill: **VisualState Named 'Selected'** on the DataTemplate root. Without the skill, models fall back to `DataTrigger` — functional but not the MAUI-idiomatic CollectionView selection pattern.

The skill also guides toward modern `Border/RoundRectangle` instead of deprecated `Frame`, and cleaner computed alignment properties rather than heavy dual-template visibility toggles.
