namespace Castle.MonoRail.Tests
{
	using Hosting;

	class StubDeploymentInfo : IDeploymentInfo
	{
		private readonly string _fspathoffset;
		private readonly string _vpath;

		public StubDeploymentInfo(string fspathoffset, string vpath)
		{
			_fspathoffset = fspathoffset;
			_vpath = vpath;
		}

		public string FSPathOffset
		{
			get { return _fspathoffset; }
		}

		public string VirtualPath
		{
			get { return _vpath; }
		}
	}
}
