using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Activation;

namespace WebApplication2
{
    // For help with this template see http://go.microsoft.com/?linkid=9737621

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]

	[DataServiceKey("Id")]
	public class Revision
	{
		public int Id { get; set; }
		public string FileName { get; set; }
		public int UserId { get; set; }
	}

	[DataServiceKey("Id")]
	public class Branch
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Revision> Revisions { get; set; }
	}

	[DataServiceKey("Id")]
	public class Repository
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Branch> Branches { get; set; }
	}

	public class MyModel
    {
		public IQueryable<Repository> Repositories { get; set; }
		public IQueryable<Branch> Branches { get; set; }
		public IQueryable<Revision> Revisions { get; set; }

	    public MyModel()
	    {
			this.Repositories = new List<Repository>()
            {
                new Repository() 
                { 
                    Id = 1, Name = "repo1", 
                    Branches = 
                        new List<Branch>
                        { 
                            new Branch() { Id = 100, Name = "Initial Spike", 
                                            Revisions = new List<Revision>()
                                                            {
                                                                new Revision() { FileName = "File1", Id = 3000, UserId = 102 }, 
                                                                new Revision() { FileName = "File2", Id = 3001, UserId = 102 },
                                                                new Revision() { FileName = "File1", Id = 3002, UserId = 101 },
                                                            }},
                            new Branch() { Id = 101, Name = "develop", 
                                            Revisions = new List<Revision>()
                                                            {
                                                                new Revision() { FileName = "File31", Id = 4000, UserId = 102 }, 
                                                                new Revision() { FileName = "File21", Id = 4001, UserId = 102 },
                                                                new Revision() { FileName = "File11", Id = 4002, UserId = 101 },
                                                            }}
                        } 
                },
            }.AsQueryable();

			this.Branches = new List<Branch>().AsQueryable();
			this.Revisions = new List<Revision>().AsQueryable();
	    }
    }

    public class AppFabricDataService1 : DataService<MyModel>
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {            
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            config.SetEntitySetAccessRule("Repositories", EntitySetRights.All);
			config.SetEntitySetAccessRule("Branches", EntitySetRights.All);
			config.SetEntitySetAccessRule("Revisions", EntitySetRights.All);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }
    }
}
