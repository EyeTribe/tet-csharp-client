/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk.Request
{
    /// <summary>
    /// Base interface for all Tracker API requests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequest : IComparable<IRequest>
    {
        int Id { set; get; }

        bool IsCancelled { get; }

        long TimeStamp { set; get; }

        int RetryAttempts { set; get; }
        
        void Cancel();

        void Finish();

        object ParseJsonResponse(JObject response);

        String ToJsonString();
    }

    /// <summary>
    /// Base class for Tracker API requests. Generics handle type of reponse
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RequestBase<T> : IRequest where T : ResponseBase
    {
        [JsonProperty(PropertyName = Protocol.KEY_CATEGORY)]
        public string Category { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_REQUEST)]
        public string Request { set; get; }

        [JsonProperty(PropertyName = Protocol.KEY_ID)]
        public int Id { set; get; }

        [JsonIgnore]
        public bool IsCancelled
        {
            get { return _Canceled; }
        }

        [JsonIgnore]
        private bool _Canceled;

        [JsonIgnore]
        public long TimeStamp
        {
            get;
            set;
        }

        [JsonIgnore]
        public int RetryAttempts
        {
            get;
            set;
        }

        [JsonIgnore]
        public Object AsyncLock;

        public RequestBase()
        {
        }

        public void Cancel()
        {
            _Canceled = true;
            Finish();
        }

        public void Finish()
        {
            if(null != AsyncLock)
            {
                lock (AsyncLock)
                {
                    Monitor.Pulse(AsyncLock);
                }
            }
        }

        public virtual object ParseJsonResponse(JObject response)
        {
            return response.ToObject<T>();
        }

        public String ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Type GetResponseType()
        {
            return typeof(T);
        }

        public override bool Equals(Object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (!(o is RequestBase<T>))
                return false;

            var other = o as RequestBase<T>;

            return Category.Equals(other.Category) &&
                this.Request.Equals(other.Request) &&
                Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hash = 283;
            hash = hash * 547 + Category.GetHashCode();
            hash = hash * 547 + Request.GetHashCode();
            hash = hash * 547 + Id.GetHashCode();
            return hash;
        }

        public int CompareTo(IRequest o)
        {
            if (ReferenceEquals(this, o))
                return 0;

            if (this.Id != 0 && o.Id == 0)
                return -1;

            if (this.Id == 0 && o.Id != 0)
                return 1;

            //if(this.id != 0 && other.id != 0)
                return this.Id < o.Id ? -1 : 1;
        }
    }
}
