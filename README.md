

# Refresh token lifecycle
1. Token generated and stored as a hash.
2. Token gets used for authentication renewal.
3. Token gets revoked when rotated or logged out.
4. Keep the record temporarily for auditing.
5. Delete after expiration and retention requirements are satisfied.

# Revoked access token lifecycle
1. JWT gets generated with a unique JTI.
2. User logs out; JTI gets stored in RevokedTokens.
3. Middleware checks whether JTI exists.
4. Keep the revocation record until JWT expiration.
5. Delete after expiration and any required retention period.


# Notes

### Concurrent Sessions & Race Conditions
- Multiple logins: Allow users to log in from multiple devices simultaneously.
- Independent sessions: Each login generates its own Access Token (JTI) and Refresh Token.
- Individual logout: Logging out from one device should not invalidate other devices.
- Refresh token rotation: Only one concurrent refresh request should successfully rotate the same refresh token.
- Concurrency protection: Implement database concurrency control to prevent duplicate token rotation.
- Future enhancement: Add session management to view and revoke individual devices.
Key takeaway: Login concurrency is allowed; refresh token rotation requires concurrency protection.

### Delete Refresh token, Revoked token only if
You can delete it when:
- It has been revoked.
- Its expiration date has passed.
- You no longer need it for auditing or security investigations.
