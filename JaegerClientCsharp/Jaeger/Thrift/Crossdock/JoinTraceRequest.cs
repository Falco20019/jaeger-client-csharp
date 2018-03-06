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
using Thrift.Protocols.Utilities;

namespace Jaeger.Thrift.Crossdock
{

  public partial class JoinTraceRequest : TBase
  {
    private Downstream _downstream;

    public string ServerRole { get; set; }

    public Downstream Downstream
    {
      get
      {
        return _downstream;
      }
      set
      {
        __isset.downstream = true;
        this._downstream = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool downstream;
    }

    public JoinTraceRequest()
    {
    }

    public JoinTraceRequest(string serverRole) : this()
    {
      this.ServerRole = serverRole;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_serverRole = false;
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
                ServerRole = await iprot.ReadStringAsync(cancellationToken);
                isset_serverRole = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.Struct)
              {
                Downstream = new Downstream();
                await Downstream.ReadAsync(iprot, cancellationToken);
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
        if (!isset_serverRole)
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
        var struc = new TStruct("JoinTraceRequest");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "serverRole";
        field.Type = TType.String;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(ServerRole, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if (Downstream != null && __isset.downstream)
        {
          field.Name = "downstream";
          field.Type = TType.Struct;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await Downstream.WriteAsync(oprot, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
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
      var sb = new StringBuilder("JoinTraceRequest(");
      sb.Append(", ServerRole: ");
      sb.Append(ServerRole);
      if (Downstream != null && __isset.downstream)
      {
        sb.Append(", Downstream: ");
        sb.Append(Downstream== null ? "<null>" : Downstream.ToString());
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}
