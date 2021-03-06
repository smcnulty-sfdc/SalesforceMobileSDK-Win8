﻿/*
 * Copyright (c) 2013, salesforce.com, inc.
 * All rights reserved.
 * Redistribution and use of this software in source and binary forms, with or
 * without modification, are permitted provided that the following conditions
 * are met:
 * - Redistributions of source code must retain the above copyright notice, this
 * list of conditions and the following disclaimer.
 * - Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 * - Neither the name of salesforce.com, inc. nor the names of its contributors
 * may be used to endorse or promote products derived from this software without
 * specific prior written permission of salesforce.com, inc.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Salesforce.SDK.Adaptation;
using Salesforce.SDK.Auth;
using Salesforce.SDK.Source.Security;
using Salesforce.SDK.Source.Settings;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Salesforce.SDK.Auth
{
    [TestClass]
    public class AuthStorageHelperTest
    {
        [TestInitialize]
        public void Setup()
        {
             EncryptionSettings settings = new EncryptionSettings(new HmacSHA256KeyGenerator())
            {
                Password = "mypassword",
                Salt = "mysalt"
            };
            Encryptor.init(settings);
        }

        [TestMethod]
        public void TestGetAuthStorageHelper()
        {
            AuthStorageHelper authStorageHelper = new AuthStorageHelper();
            Assert.IsNotNull(authStorageHelper);
        }

        [TestMethod]
        public void TestPersistRetrieveDeleteCredentials()
        {
            Account account = new Account("loginUrl", "clientId", "callbackUrl", new string[] { "scopeA", "scopeB" }, "instanceUrl", "identityUrl", "accessToken", "refreshToken");
            account.UserId = "userId";
            account.UserName = "userName";
            AuthStorageHelper authStorageHelper = new AuthStorageHelper();
            CheckAccount(account, false);
            TypeInfo auth = authStorageHelper.GetType().GetTypeInfo();
            MethodInfo persist = auth.GetDeclaredMethod("PersistCredentials");
            MethodInfo delete = auth.GetDeclaredMethods("DeletePersistedCredentials").First(method => method.GetParameters().Count() == 1);
            persist.Invoke(authStorageHelper, new object[] { account });
            CheckAccount(account, true);
            delete.Invoke(authStorageHelper, new object[] { account.UserId });
            CheckAccount(account, false);
        }

        private void CheckAccount(Account expectedAccount, bool exists)
        {
            AuthStorageHelper authStorageHelper = new AuthStorageHelper();
            TypeInfo auth = authStorageHelper.GetType().GetTypeInfo();
            MethodInfo retrieve = auth.GetDeclaredMethod("RetrievePersistedCredentials");
            var accounts = (Dictionary<string, Account>) retrieve.Invoke(authStorageHelper, null);
            if (!exists)
            {
                Assert.IsFalse(accounts.ContainsKey(expectedAccount.UserId), "Account " + expectedAccount.UserId + " should not have been found");
            }
            else
            {
                Assert.IsTrue(accounts.ContainsKey(expectedAccount.UserId),  "Account " + expectedAccount.UserId + " should exist");
                Account account = accounts[expectedAccount.UserId];
                Assert.AreEqual(expectedAccount.LoginUrl, account.LoginUrl);
                Assert.AreEqual(expectedAccount.ClientId, account.ClientId);
                Assert.AreEqual(expectedAccount.CallbackUrl, account.CallbackUrl);
                Assert.AreEqual(expectedAccount.Scopes.Length, account.Scopes.Length);
                Assert.AreEqual(expectedAccount.InstanceUrl, account.InstanceUrl);
                Assert.AreEqual(expectedAccount.AccessToken, expectedAccount.AccessToken);
                Assert.AreEqual(expectedAccount.RefreshToken, expectedAccount.RefreshToken);
            }
        }
    }
}
