
        
        var mrRoutes = {};

        function initializeRouteModule(vpath) {
            vpath = vpath === '/' ? '' : vpath;

            function appendNamespace (namespaceString) {
                var parts = namespaceString.split('.'),
                    parent = mrRoutes,
                    currentPart = '';    

                for (var i = 0, length = parts.length; i < length; i++) {
                    currentPart = parts[i];
                    parent[currentPart] = parent[currentPart] || {};
                    parent = parent[currentPart];
                }

                return parent;
            }
            var Root = {
                
                ActionWithRedirect: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Root/ActionWithRedirect' : '/Root/ActionWithRedirect?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Root/ActionWithRedirect'; },
                    del: function() { return vpath + '/Root/ActionWithRedirect'; },
                    put: function() { return vpath + '/Root/ActionWithRedirect'; }
                },

                ActionWithRedirect2: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Root/ActionWithRedirect2' : '/Root/ActionWithRedirect2?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Root/ActionWithRedirect2'; },
                    del: function() { return vpath + '/Root/ActionWithRedirect2'; },
                    put: function() { return vpath + '/Root/ActionWithRedirect2'; }
                },

                ActionWithRedirectPerm: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Root/ActionWithRedirectPerm' : '/Root/ActionWithRedirectPerm?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Root/ActionWithRedirectPerm'; },
                    del: function() { return vpath + '/Root/ActionWithRedirectPerm'; },
                    put: function() { return vpath + '/Root/ActionWithRedirectPerm'; }
                },

                ActionWithRedirectPerm2: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Root/ActionWithRedirectPerm2' : '/Root/ActionWithRedirectPerm2?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Root/ActionWithRedirectPerm2'; },
                    del: function() { return vpath + '/Root/ActionWithRedirectPerm2'; },
                    put: function() { return vpath + '/Root/ActionWithRedirectPerm2'; }
                },

                Index: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/' : '/?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/'; },
                    del: function() { return vpath + '/'; },
                    put: function() { return vpath + '/'; }
                },

                ReplyWith304: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Root/ReplyWith304' : '/Root/ReplyWith304?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Root/ReplyWith304'; },
                    del: function() { return vpath + '/Root/ReplyWith304'; },
                    put: function() { return vpath + '/Root/ReplyWith304'; }
                }
            };
        
            var ns = appendNamespace('WebSiteForIntegration.Controllers');

            ns.Root = Root;
        
        }
