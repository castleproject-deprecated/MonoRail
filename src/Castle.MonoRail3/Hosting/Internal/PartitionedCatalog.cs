namespace Castle.MonoRail3.Hosting.Internal
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition.Primitives;
	using System.Linq;
	using System.Threading;

	class PartitionedCatalog : ComposablePartCatalog
	{
		private readonly object locker = new object();

		private readonly Predicate<ComposablePartDefinition> filter;

		private readonly ComposablePartCatalog source;
		private IQueryable<ComposablePartDefinition> parts;
		private ComposablePartCatalog complement;

		public PartitionedCatalog(ComposablePartCatalog source, Predicate<ComposablePartDefinition> filter)
		{
			this.source = source;
			this.filter = filter;
		}

		public override IQueryable<ComposablePartDefinition> Parts
		{
			get
			{
				if (parts == null)
				{
					FilterIt();
				}

				return parts;
			}
		}

		private void FilterIt()
		{
			lock (locker)
			{
				if (parts == null)
				{
					parts = FilterCatalog();
					Thread.MemoryBarrier();
				}
			}
		}

		public ComposablePartCatalog Complement
		{
			get
			{
				if (complement == null)
				{
					FilterIt();
				}

				return complement;
			}
		}

		private IQueryable<ComposablePartDefinition> FilterCatalog()
		{
			var trueSet = new List<ComposablePartDefinition>();
			var falseSet = new List<ComposablePartDefinition>();

			foreach (var part in source.Parts)
			{
				if (filter(part))
					trueSet.Add(part);
				else
					falseSet.Add(part);
			}

			complement = new BasicCatalog(falseSet.AsQueryable());

			return trueSet.AsQueryable();
		}

		class BasicCatalog : ComposablePartCatalog
		{
			private readonly IQueryable<ComposablePartDefinition> parts;

			public BasicCatalog(IQueryable<ComposablePartDefinition> parts)
			{
				this.parts = parts;
			}

			public override IQueryable<ComposablePartDefinition> Parts
			{
				get { return parts; }
			}
		}
	}
}
