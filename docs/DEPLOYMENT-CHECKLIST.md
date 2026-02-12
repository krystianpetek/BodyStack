# BodyStack Refactoring - Deployment Checklist

## Pre-Deployment

### Code Review
- [ ] All critical fixes reviewed by Tech Lead
- [ ] SignalR changes approved by Security Engineer
- [ ] Database query changes tested with realistic data volumes

### Testing
- [ ] All existing tests pass (`dotnet test`)
- [ ] New exception tests pass
- [ ] Repository tests pass with in-memory database
- [ ] Integration tests pass (if environment available)
- [ ] Frontend tests pass (`npm run test:run` in bodystack.client)
- [ ] Manual testing completed (see below)

### Database
- [ ] No schema migrations required (confirmed)
- [ ] Review query execution plans for optimized queries
- [ ] Verify indexes on `FitatuSessions.UpdatedAt`

## Deployment Steps

### Phase 1: Backend Critical (Deploy First)
1. Deploy updated backend code
2. Verify SignalR connections are working
3. Check error logs for any new exception types
4. Monitor database query performance

### Phase 2: Frontend (After backend stable)
1. Deploy updated frontend code
2. Verify SignalR group joining works correctly
3. Check browser console for errors

## Manual Testing

### Exception Handling
- [ ] Fitatu session expired → triggers login flow
- [ ] Month export incomplete → shows missing days
- [ ] API errors → user-friendly messages

### SignalR Isolation
- [ ] User A recalculation → User A sees progress
- [ ] User B (different browser) → doesn't see User A's progress
- [ ] Reconnection → automatically rejoins group

### Database Queries
- [ ] Load Fitatu page → sessions load quickly
- [ ] Month recalculation → no timeout errors
- [ ] Export CSV → completes in reasonable time

### Frontend Components
- [ ] Integration selector switches correctly
- [ ] Fitatu login form works
- [ ] Suunto login form works
- [ ] Dashboard navigation works

## Rollback Plan

### If Issues Detected
1. **Immediate**: Revert to previous deployment
2. **SignalR issues**: Can temporarily revert to broadcast mode
3. **Database issues**: Rollback code (no schema changes to revert)
4. **Frontend issues**: Revert client build

### Monitoring (Post-Deployment)
- [ ] SignalR connection errors (should be minimal)
- [ ] Database query duration (should improve)
- [ ] Memory usage (should decrease)
- [ ] Error rates (should not increase)

## Verification Commands

```bash
# Backend tests
dotnet test BodyStack.Server.Tests/

# Frontend tests
cd bodystack.client && npm run test:run

# Build verification
dotnet build BodyStack.Server/
cd bodystack.client && npm run build

# Health check
curl https://your-api/api/health
```

## Sign-Off

| Role | Name | Date | Status |
|------|------|------|--------|
| Tech Lead | | | |
| QA Engineer | | | |
| Security Engineer | | | |
| Product Owner | | | |
