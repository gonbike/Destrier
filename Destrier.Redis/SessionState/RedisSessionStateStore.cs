﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace Destrier.Redis.SessionState
{
    public class RedisSessionStateStore : SessionStateStoreProviderBase
    {
        private String _applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
        public String ApplicationName { get { return _applicationName; } set { _applicationName = value; } }

        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Password { get; set; }
        
        public String StoreKeyPrefix { get { return String.Format("RedisSessionStateStore:{0}", this.ApplicationName); } }

        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
        {
            var newStore = new RedisSessionStateItem()
            {
                SessionId = id,
                ApplicationName = ApplicationName,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes((double)timeout),
                Locked = false,
                Timeout = timeout,
                Flags = 1
            };

            var fullKey = String.Format("{0}:{1}", this.StoreKeyPrefix, id);

            using (var rc = new RedisClient(Host, Port, Password))
            {
                RedisBinarySerializer.Serialize(fullKey, newStore, rc);
            }
        }

        public override void Dispose() { }

        public override void InitializeRequest(System.Web.HttpContext context) { }

        public override void EndRequest(System.Web.HttpContext context) { }

        public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            throw new NotImplementedException();
        }

        public override void ResetItemTimeout(System.Web.HttpContext context, string id)
        {
            throw new NotImplementedException();
        }

        public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            throw new NotImplementedException();
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }
    }
}