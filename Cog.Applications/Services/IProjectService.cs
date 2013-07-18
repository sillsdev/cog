using System;
using SIL.Cog.Domain;

namespace SIL.Cog.Applications.Services
{
	public interface IProjectService
	{
		event EventHandler<EventArgs> ProjectOpened;

		void Init();
		bool New();
		bool Open();
		bool Close();
		bool Save();
		bool SaveAs();

		bool IsChanged { get; }
		CogProject Project { get; }
		string ProjectName { get; }
	}
}
