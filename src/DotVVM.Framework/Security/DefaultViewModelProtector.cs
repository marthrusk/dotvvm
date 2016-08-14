using System;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.Owin.Security.DataProtection;

namespace DotVVM.Framework.Security
{
    /// <summary>
    /// Cryptographically protects serialized part of ViewModel with key derived from master key,
    /// with request identity (full URI) and User identity (user name, if authenticated).
    /// </summary>
    public class DefaultViewModelProtector : IViewModelProtector
    {
        private const string PRIMARY_PURPOSE = "DotVVM.Framework.Security.DefaultViewModelProtector";

        private IDataProtectionProvider protectionProvider;

        public DefaultViewModelProtector(DotvvmConfiguration configuration)
        {
            this.protectionProvider = configuration.ServiceLocator.GetService<IDataProtectionProvider>();
        }

        public string Protect(string serializedData, IDotvvmRequestContext context)
        {
            if (serializedData == null) throw new ArgumentNullException(nameof(serializedData));
            if (string.IsNullOrWhiteSpace(serializedData)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(serializedData));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.Create(PRIMARY_PURPOSE, userIdentity, requestIdentity);

            // Return protected view model data
            var dataToProtect = Encoding.UTF8.GetBytes(serializedData);
            var protectedData = protector.Protect(dataToProtect);
            return Convert.ToBase64String(protectedData);
        }

        public string Unprotect(string protectedData, IDotvvmRequestContext context)
        {
            if (protectedData == null) throw new ArgumentNullException(nameof(protectedData));
            if (string.IsNullOrWhiteSpace(protectedData)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(protectedData));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.Create(PRIMARY_PURPOSE, userIdentity, requestIdentity);

            // Return unprotected view model data
            var dataToUnprotect = Convert.FromBase64String(protectedData);
            var unprotectedData = protector.Unprotect(dataToUnprotect);
            return Encoding.UTF8.GetString(unprotectedData);
        }

    }
}