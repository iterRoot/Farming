using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace FarmingApi.Core;

public abstract class Entity
{
	public Guid GuidId { get; set; } = Guid.NewGuid();
	
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	
}
public abstract class AuditableEntity : Entity
{

	public DateTime CreatedAt { get; set; }
	public Guid? CreatedBy { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public Guid? UpdatedBy { get; set; }
	public DateTime? DeletedAt { get; set; }
	public Guid? DeletedBy { get; set; }
	public bool? InActive { get; set; }
}