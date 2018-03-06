/**
 * Autogenerated by Thrift Compiler (0.11.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using Thrift.Protocols.Entities;

namespace Jaeger.Thrift.Agent
{

  public partial class OperationSamplingStrategy : TBase
  {

    public string Operation { get; set; }

    public ProbabilisticSamplingStrategy ProbabilisticSampling { get; set; }

    public OperationSamplingStrategy()
    {
    }

    public OperationSamplingStrategy(string operation, ProbabilisticSamplingStrategy probabilisticSampling) : this()
    {
      this.Operation = operation;
      this.ProbabilisticSampling = probabilisticSampling;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_operation = false;
        bool isset_probabilisticSampling = false;
        TField field;
        await iprot.ReadStructBeginAsync(cancellationToken);
        while (true)
        {
          field = await iprot.ReadFieldBeginAsync(cancellationToken);
          if (field.Type == TType.Stop)
          {
            break;
          }

          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.String)
              {
                Operation = await iprot.ReadStringAsync(cancellationToken);
                isset_operation = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.Struct)
              {
                ProbabilisticSampling = new ProbabilisticSamplingStrategy();
                await ProbabilisticSampling.ReadAsync(iprot, cancellationToken);
                isset_probabilisticSampling = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            default: 
              await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              break;
          }

          await iprot.ReadFieldEndAsync(cancellationToken);
        }

        await iprot.ReadStructEndAsync(cancellationToken);
        if (!isset_operation)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_probabilisticSampling)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
    {
      oprot.IncrementRecursionDepth();
      try
      {
        var struc = new TStruct("OperationSamplingStrategy");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "operation";
        field.Type = TType.String;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(Operation, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "probabilisticSampling";
        field.Type = TType.Struct;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await ProbabilisticSampling.WriteAsync(oprot, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        await oprot.WriteFieldStopAsync(cancellationToken);
        await oprot.WriteStructEndAsync(cancellationToken);
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString()
    {
      var sb = new StringBuilder("OperationSamplingStrategy(");
      sb.Append(", Operation: ");
      sb.Append(Operation);
      sb.Append(", ProbabilisticSampling: ");
      sb.Append(ProbabilisticSampling== null ? "<null>" : ProbabilisticSampling.ToString());
      sb.Append(")");
      return sb.ToString();
    }
  }

}
