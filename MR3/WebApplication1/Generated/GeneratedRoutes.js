
        
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
            var Home = {
                
                Components: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Home/Components' : '/Home/Components?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Home/Components'; },
                    del: function() { return vpath + '/Home/Components'; },
                    put: function() { return vpath + '/Home/Components'; }
                },

                Index: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Home' : '/Home?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Home'; },
                    del: function() { return vpath + '/Home'; },
                    put: function() { return vpath + '/Home'; }
                }
            };
        
            var ns = appendNamespace('WebApplication1.Controllers');

            ns.Home = Home;
        
            var Todo = {
                
                Create: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/Create' : '/Todo/Create?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/Create'; },
                    del: function() { return vpath + '/Todo/Create'; },
                    put: function() { return vpath + '/Todo/Create'; }
                },

                DeleteConfirmation: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/DeleteConfirmation' : '/Todo/DeleteConfirmation?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/DeleteConfirmation'; },
                    del: function() { return vpath + '/Todo/DeleteConfirmation'; },
                    put: function() { return vpath + '/Todo/DeleteConfirmation'; }
                },

                Edit: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/Edit' : '/Todo/Edit?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/Edit'; },
                    del: function() { return vpath + '/Todo/Edit'; },
                    put: function() { return vpath + '/Todo/Edit'; }
                },

                Index: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/' : '/?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/'; },
                    del: function() { return vpath + '/'; },
                    put: function() { return vpath + '/'; }
                },

                New: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/New' : '/Todo/New?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/New'; },
                    del: function() { return vpath + '/Todo/New'; },
                    put: function() { return vpath + '/Todo/New'; }
                },

                Remove: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/Remove' : '/Todo/Remove?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/Remove'; },
                    del: function() { return vpath + '/Todo/Remove'; },
                    put: function() { return vpath + '/Todo/Remove'; }
                },

                Update: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/Update' : '/Todo/Update?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/Update'; },
                    del: function() { return vpath + '/Todo/Update'; },
                    put: function() { return vpath + '/Todo/Update'; }
                },

                View: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/Todo/View' : '/Todo/View?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/Todo/View'; },
                    del: function() { return vpath + '/Todo/View'; },
                    put: function() { return vpath + '/Todo/View'; }
                }
            };
        
            var ns = appendNamespace('WebApplication1.Controllers');

            ns.Todo = Todo;
        
            var User = {
                
                Confirmation: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Confirmation' : '/User/Confirmation?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Confirmation'; },
                    del: function() { return vpath + '/User/Confirmation'; },
                    put: function() { return vpath + '/User/Confirmation'; }
                },

                Create: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Create' : '/User/Create?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Create'; },
                    del: function() { return vpath + '/User/Create'; },
                    put: function() { return vpath + '/User/Create'; }
                },

                Edit: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Edit' : '/User/Edit?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Edit'; },
                    del: function() { return vpath + '/User/Edit'; },
                    put: function() { return vpath + '/User/Edit'; }
                },

                Index: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User' : '/User?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User'; },
                    del: function() { return vpath + '/User'; },
                    put: function() { return vpath + '/User'; }
                },

                New: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/New' : '/User/New?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/New'; },
                    del: function() { return vpath + '/User/New'; },
                    put: function() { return vpath + '/User/New'; }
                },

                Remove: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Remove' : '/User/Remove?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Remove'; },
                    del: function() { return vpath + '/User/Remove'; },
                    put: function() { return vpath + '/User/Remove'; }
                },

                Role: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Role' : '/User/Role?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Role'; },
                    del: function() { return vpath + '/User/Role'; },
                    put: function() { return vpath + '/User/Role'; }
                },

                Update: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/Update' : '/User/Update?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/Update'; },
                    del: function() { return vpath + '/User/Update'; },
                    put: function() { return vpath + '/User/Update'; }
                },

                View: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/User/View' : '/User/View?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/User/View'; },
                    del: function() { return vpath + '/User/View'; },
                    put: function() { return vpath + '/User/View'; }
                }
            };
        
            var ns = appendNamespace('WebApplication1.Controllers');

            ns.User = User;
        
            var OrdersComponent = {
                
                Refresh: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/viewcomponents/OrdersComponent/Refresh' : '/viewcomponents/OrdersComponent/Refresh?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/viewcomponents/OrdersComponent/Refresh'; },
                    del: function() { return vpath + '/viewcomponents/OrdersComponent/Refresh'; },
                    put: function() { return vpath + '/viewcomponents/OrdersComponent/Refresh'; }
                },

                Render: {
                    get: function (params) {
                        return vpath + (params == undefined ? '/viewcomponents/OrdersComponent/Render' : '/viewcomponents/OrdersComponent/Render?' + jQuery.param(params));
                    },
                    post: function() { return vpath + '/viewcomponents/OrdersComponent/Render'; },
                    del: function() { return vpath + '/viewcomponents/OrdersComponent/Render'; },
                    put: function() { return vpath + '/viewcomponents/OrdersComponent/Render'; }
                }
            };
        
            var ns = appendNamespace('WebApplication1.ViewComponents.Components');

            ns.OrdersComponent = OrdersComponent;
        
        }
