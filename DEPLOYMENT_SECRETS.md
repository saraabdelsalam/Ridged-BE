# Deployment & Secrets Configuration Guide

## ✅ Current Setup (Development)

Your seller password is now stored in **User Secrets** and will **automatically be hashed** by `UserManager.CreateAsync()` before being stored in the database.

### Password Hashing
- **UserManager** uses **PBKDF2** (Password-Based Key Derivation Function 2) by default
- Hashing happens automatically when you call `CreateAsync(user, password)`
- The plain text password "Seller@123" is **never** stored in the database
- Only the hashed version is stored in the `Users.PasswordHash` column

---

## 🔒 Development Environment (Current Setup)

### User Secrets (Already Configured)
```bash
# View stored secrets
dotnet user-secrets list --project RidgedApi

# Add/Update a secret
dotnet user-secrets set "DefaultAccounts:Seller:Password" "YourNewPassword" --project RidgedApi

# Remove a secret
dotnet user-secrets remove "DefaultAccounts:Seller:Password" --project RidgedApi
```

### Where are User Secrets Stored?
- Windows: `%APPDATA%\Microsoft\UserSecrets\684d946f-cc84-4ba5-ba43-1667ea1a015d\secrets.json`
- macOS/Linux: `~/.microsoft/usersecrets/684d946f-cc84-4ba5-ba43-1667ea1a015d/secrets.json`

**These files are NOT in your Git repository!** ✅

---

## 🚀 Production Deployment Options

### Option 1: Environment Variables (Simple)

**Azure App Service / AWS Elastic Beanstalk / Docker:**
```bash
# Set environment variable
DefaultAccounts__Seller__Password=YourStrongPassword@123

# Note: Use double underscores (__) for nested configuration in environment variables
```

**Azure App Service (Portal):**
1. Go to your App Service
2. Configuration → Application Settings
3. Add new setting:
   - Name: `DefaultAccounts__Seller__Password`
   - Value: `YourStrongPassword@123`
4. Save

**Docker Compose:**
```yaml
services:
  ridgedapi:
    image: ridgedapi:latest
    environment:
      - DefaultAccounts__Seller__Password=YourStrongPassword@123
      # Or use docker secrets for better security
    env_file:
      - .env.production  # This file should NOT be in Git
```

---

### Option 2: Azure Key Vault (Recommended for Production)

**1. Install NuGet Package:**
```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

**2. Update Program.cs:**
```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault in production
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
}

// ... rest of your configuration
```

**3. Store Secret in Azure Key Vault:**
```bash
# Azure CLI
az keyvault secret set \
  --vault-name your-keyvault-name \
  --name DefaultAccounts--Seller--Password \
  --value "YourStrongPassword@123"
```

**4. Add to appsettings.json:**
```json
{
  "KeyVault": {
    "Url": "https://your-keyvault-name.vault.azure.net/"
  }
}
```

---

### Option 3: AWS Secrets Manager

**1. Install NuGet Package:**
```bash
dotnet add package Amazon.Extensions.Configuration.SystemsManager
```

**2. Update Program.cs:**
```csharp
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddSystemsManager("/ridged-api/");
}
```

**3. Store Secret in AWS:**
```bash
aws secretsmanager create-secret \
  --name /ridged-api/DefaultAccounts/Seller/Password \
  --secret-string "YourStrongPassword@123"
```

---

### Option 4: Kubernetes Secrets

**1. Create Secret:**
```bash
kubectl create secret generic seller-credentials \
  --from-literal=password=YourStrongPassword@123
```

**2. Mount in Deployment:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ridged-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: ridgedapi:latest
        env:
        - name: DefaultAccounts__Seller__Password
          valueFrom:
            secretKeyRef:
              name: seller-credentials
              key: password
```

---

## 🔐 Security Best Practices

### ✅ DO:
- Use User Secrets for local development
- Use Azure Key Vault, AWS Secrets Manager, or similar for production
- Use strong passwords (12+ characters, mixed case, numbers, symbols)
- Rotate passwords regularly
- Use Managed Identity in cloud environments
- Enable audit logging for secret access

### ❌ DON'T:
- Store passwords in appsettings.json (even hashed ones)
- Commit secrets to Git
- Share secrets via email or chat
- Use weak default passwords
- Hardcode passwords in migration files

---

## 🧪 Testing Password Hashing

To verify the password is being hashed, check your database:

```sql
SELECT Id, Email, PasswordHash FROM Users WHERE Email = 'saraalabd777777@gmail.com';
```

The `PasswordHash` should look like:
```
AQAAAAIAAYagAAAAEJ1x4H8vN7+KpqR... (very long hashed string)
```

**NOT** like: `Seller@123` ✅

---

## 📝 Current Configuration Priority (ASP.NET Core)

Configuration is loaded in this order (later overrides earlier):

1. **appsettings.json** - Base configuration
2. **appsettings.{Environment}.json** - Environment-specific
3. **User Secrets** (Development only) - Current setup ✅
4. **Environment Variables** - Production
5. **Command-line arguments** - Overrides

So your User Secret will override the default value in your seeder! ✅

---

## 🎯 Recommended Setup by Environment

| Environment | Secrets Storage | Configuration |
|-------------|----------------|---------------|
| **Local Dev** | User Secrets | ✅ Already configured |
| **CI/CD** | Environment Variables | Set in pipeline |
| **Azure** | Azure Key Vault | Use Managed Identity |
| **AWS** | AWS Secrets Manager | Use IAM roles |
| **Docker** | Docker Secrets / .env | Mount as volume |
| **Kubernetes** | K8s Secrets | Mount as env vars |

---

## 🚨 Emergency: Password Leaked?

If a password is accidentally committed to Git:

1. **Rotate immediately** - Change the password in all environments
2. **Update Git history** (if recent):
   ```bash
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch path/to/file" HEAD
   ```
3. **Force push** (coordinate with team)
4. **Invalidate all tokens** - Force users to re-login
5. **Review access logs** - Check for unauthorized access

---

## 📞 Support

For questions about secrets management:
- Azure Key Vault: https://learn.microsoft.com/azure/key-vault/
- AWS Secrets Manager: https://docs.aws.amazon.com/secretsmanager/
- User Secrets: https://learn.microsoft.com/aspnet/core/security/app-secrets
