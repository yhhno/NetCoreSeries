// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Microsoft.AspNetCore.Http
{
    public class DefaultHttpContextTests
    {
        [Fact]
        public void GetOnSessionProperty_ThrowsOnMissingSessionFeature()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => context.Session);
            Assert.Equal("Session has not been configured for this application or request.", exception.Message);
        }

        [Fact]
        public void GetOnSessionProperty_ReturnsAvailableSession()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var session = new TestSession();
            session.Set("key1", null);
            session.Set("key2", null);
            var feature = new BlahSessionFeature();
            feature.Session = session;
            context.Features.Set<ISessionFeature>(feature);

            // Act & Assert
            Assert.Same(session, context.Session);
            context.Session.Set("key3", null);
            Assert.Equal(3, context.Session.Keys.Count());
        }

        [Fact]
        public void AllowsSettingSession_WithoutSettingUpSessionFeature_Upfront()
        {
            // Arrange
            var session = new TestSession();
            var context = new DefaultHttpContext();

            // Act
            context.Session = session;

            // Assert
            Assert.Same(session, context.Session);
        }

        [Fact]
        public void SettingSession_OverridesAvailableSession()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var session = new TestSession();
            session.Set("key1", null);
            session.Set("key2", null);
            var feature = new BlahSessionFeature();
            feature.Session = session;
            context.Features.Set<ISessionFeature>(feature);

            // Act
            context.Session = new TestSession();

            // Assert
            Assert.NotSame(session, context.Session);
            Assert.Empty(context.Session.Keys);
        }

        [Fact]
        public void EmptyUserIsNeverNull()
        {
            var context = new DefaultHttpContext(new FeatureCollection());
            Assert.NotNull(context.User);
            Assert.Single(context.User.Identities);
            Assert.True(object.ReferenceEquals(context.User, context.User));
            Assert.False(context.User.Identity.IsAuthenticated);
            Assert.True(string.IsNullOrEmpty(context.User.Identity.AuthenticationType));

            context.User = null;
            Assert.NotNull(context.User);
            Assert.Single(context.User.Identities);
            Assert.True(object.ReferenceEquals(context.User, context.User));
            Assert.False(context.User.Identity.IsAuthenticated);
            Assert.True(string.IsNullOrEmpty(context.User.Identity.AuthenticationType));

            context.User = new ClaimsPrincipal();
            Assert.NotNull(context.User);
            Assert.Empty(context.User.Identities);
            Assert.True(object.ReferenceEquals(context.User, context.User));
            Assert.Null(context.User.Identity);

            context.User = new ClaimsPrincipal(new ClaimsIdentity("SomeAuthType"));
            Assert.Equal("SomeAuthType", context.User.Identity.AuthenticationType);
            Assert.True(context.User.Identity.IsAuthenticated);
        }

        [Fact]
        public void GetItems_DefaultCollectionProvided()
        {
            var context = new DefaultHttpContext(new FeatureCollection());
            Assert.Null(context.Features.Get<IItemsFeature>());
            var items = context.Items;
            Assert.NotNull(context.Features.Get<IItemsFeature>());
            Assert.NotNull(items);
            Assert.Same(items, context.Items);
            var item = new object();
            context.Items["foo"] = item;
            Assert.Same(item, context.Items["foo"]);
        }

        [Fact]
        public void GetItems_DefaultRequestIdentifierAvailable()
        {
            var context = new DefaultHttpContext(new FeatureCollection());
            Assert.Null(context.Features.Get<IHttpRequestIdentifierFeature>());
            var traceIdentifier = context.TraceIdentifier;
            Assert.NotNull(context.Features.Get<IHttpRequestIdentifierFeature>());
            Assert.NotNull(traceIdentifier);
            Assert.Same(traceIdentifier, context.TraceIdentifier);

            context.TraceIdentifier = "Hello";
            Assert.Same("Hello", context.TraceIdentifier);
        }

        [Fact]
        public void SetItems_NewCollectionUsed()
        {
            var context = new DefaultHttpContext(new FeatureCollection());
            Assert.Null(context.Features.Get<IItemsFeature>());
            var items = new Dictionary<object, object>();
            context.Items = items;
            Assert.NotNull(context.Features.Get<IItemsFeature>());
            Assert.Same(items, context.Items);
            var item = new object();
            items["foo"] = item;
            Assert.Same(item, context.Items["foo"]);
        }

        [Fact]
        public void UpdateFeatures_ClearsCachedFeatures()
        {
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature());
            features.Set<IHttpResponseFeature>(new HttpResponseFeature());
            features.Set<IHttpWebSocketFeature>(new TestHttpWebSocketFeature());

            // featurecollection is set. all cached interfaces are null.
            var context = new DefaultHttpContext(features);
            TestAllCachedFeaturesAreNull(context, features);
            Assert.Equal(3, features.Count());

            // getting feature properties populates feature collection with defaults
            TestAllCachedFeaturesAreSet(context, features);
            Assert.NotEqual(3, features.Count());

            // featurecollection is null. and all cached interfaces are null.
            // only top level is tested because child objects are inaccessible.
            context.Uninitialize();
            TestCachedFeaturesAreNull(context, null);


            var newFeatures = new FeatureCollection();
            newFeatures.Set<IHttpRequestFeature>(new HttpRequestFeature());
            newFeatures.Set<IHttpResponseFeature>(new HttpResponseFeature());
            newFeatures.Set<IHttpWebSocketFeature>(new TestHttpWebSocketFeature());

            // featurecollection is set to newFeatures. all cached interfaces are null.
            context.Initialize(newFeatures);
            TestAllCachedFeaturesAreNull(context, newFeatures);
            Assert.Equal(3, newFeatures.Count());

            // getting feature properties populates new feature collection with defaults
            TestAllCachedFeaturesAreSet(context, newFeatures);
            Assert.NotEqual(3, newFeatures.Count());
        }

        void TestAllCachedFeaturesAreNull(HttpContext context, IFeatureCollection features)
        {
            TestCachedFeaturesAreNull(context, features);
            TestCachedFeaturesAreNull(context.Request, features);
            TestCachedFeaturesAreNull(context.Response, features);
#pragma warning disable CS0618 // Type or member is obsolete
            TestCachedFeaturesAreNull(context.Authentication, features);
#pragma warning restore CS0618 // Type or member is obsolete
            TestCachedFeaturesAreNull(context.Connection, features);
            TestCachedFeaturesAreNull(context.WebSockets, features);
        }

        void TestCachedFeaturesAreNull(object value, IFeatureCollection features)
        {
            var type = value.GetType();

            var field = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(f =>
                    f.FieldType.GetTypeInfo().IsGenericType &&
                    f.FieldType.GetGenericTypeDefinition() == typeof(FeatureReferences<>));

            var boxedExpectedStruct = features == null ?
                Activator.CreateInstance(field.FieldType) :
                Activator.CreateInstance(field.FieldType, features);

            var boxedActualStruct = field.GetValue(value);

            Assert.Equal(boxedExpectedStruct, boxedActualStruct);
        }

        void TestAllCachedFeaturesAreSet(HttpContext context, IFeatureCollection features)
        {
            TestCachedFeaturesAreSet(context, features);
            TestCachedFeaturesAreSet(context.Request, features);
            TestCachedFeaturesAreSet(context.Response, features);
#pragma warning disable CS0618 // Type or member is obsolete
            TestCachedFeaturesAreSet(context.Authentication, features);
#pragma warning restore CS0618 // Type or member is obsolete
            TestCachedFeaturesAreSet(context.Connection, features);
            TestCachedFeaturesAreSet(context.WebSockets, features);
        }

        void TestCachedFeaturesAreSet(object value, IFeatureCollection features)
        {
            var type = value.GetType();

            var properties = type
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.GetTypeInfo().IsInterface);

            TestFeatureProperties(value, features, properties);

            var fields = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.FieldType.GetTypeInfo().IsInterface);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(IFeatureCollection))
                {
                    Assert.Same(features, field.GetValue(value));
                }
                else
                {
                    var v = field.GetValue(value);
                    Assert.Same(features[field.FieldType], v);
                    Assert.NotNull(v);
                }
            }

        }

        private static void TestFeatureProperties(object value, IFeatureCollection features, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(IFeatureCollection))
                {
                    Assert.Same(features, property.GetValue(value));
                }
                else
                {
                    if (property.Name.Contains("Feature"))
                    {
                        var v = property.GetValue(value);
                        Assert.Same(features[property.PropertyType], v);
                        Assert.NotNull(v);
                    }
                }
            }
        }

        private HttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            return context;
        }

        private class TestSession : ISession
        {
            private Dictionary<string, byte[]> _store
                = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            public string Id { get; set; }

            public bool IsAvailable { get; } = true;

            public IEnumerable<string> Keys { get { return _store.Keys; } }

            public void Clear()
            {
                _store.Clear();
            }

            public Task CommitAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }

            public Task LoadAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }

            public void Remove(string key)
            {
                _store.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _store[key] = value;
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return _store.TryGetValue(key, out value);
            }
        }

        private class BlahSessionFeature : ISessionFeature
        {
            public ISession Session { get; set; }
        }

        private class TestHttpWebSocketFeature : IHttpWebSocketFeature
        {
            public bool IsWebSocketRequest
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Task<WebSocket> AcceptAsync(WebSocketAcceptContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}