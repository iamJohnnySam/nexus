using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ResourceBlockLink : INotifyPropertyChanged
{
	private List<ResourceBlock> blocks;
	public List<ResourceBlock> Blocks
	{
		get
		{
			return blocks;
		}
		set
		{
			blocks = value;
			OnPropertyChanged();
		}
	}

    public List<ResourceBlock> ResourceBlocks_ME { get { return Blocks.Where(o => o.Employee?.DesignationId == 2).ToList(); } }
    public List<ResourceBlock> ResourceBlocks_EE { get { return Blocks.Where(o => o.Employee?.DesignationId == 4).ToList(); } }
    public List<ResourceBlock> ResourceBlocks_PD { get { return Blocks.Where(o => (o.Employee?.DesignationId == 3 || o.Employee?.DesignationId == 6)).ToList(); } }
    public List<ResourceBlock> ResourceBlocks_SE { get { return Blocks.Where(o => o.Employee?.DesignationId == 1).ToList(); } }
    public List<ResourceBlock> ResourceBlocks_PR { get { return Blocks.Where(o => (o.Employee?.DesignationId == 5 || o.Employee?.DesignationId == 7)).ToList(); } }

    public ResourceBlockLink(List<ResourceBlock> blocks)
    {
        this.blocks = blocks;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
