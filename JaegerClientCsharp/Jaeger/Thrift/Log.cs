/**
 * Autogenerated by Thrift Compiler (0.11.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using Thrift.Protocols.Entities;
using Thrift.Protocols.Utilities;

namespace Jaeger.Thrift
{

  public partial class Log : TBase
  {

    public long Timestamp { get; set; }

    public List<Tag> Fields { get; set; }

    public Log()
    {
    }

    public Log(long timestamp, List<Tag> fields) : this()
    {
      this.Timestamp = timestamp;
      this.Fields = fields;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_timestamp = false;
        bool isset_fields = false;
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
              if (field.Type == TType.I64)
              {
                Timestamp = await iprot.ReadI64Async(cancellationToken);
                isset_timestamp = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.List)
              {
                {
                  Fields = new List<Tag>();
                  TList _list0 = await iprot.ReadListBeginAsync(cancellationToken);
                  for(int _i1 = 0; _i1 < _list0.Count; ++_i1)
                  {
                    Tag _elem2;
                    _elem2 = new Tag();
                    await _elem2.ReadAsync(iprot, cancellationToken);
                    Fields.Add(_elem2);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
                isset_fields = true;
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
        if (!isset_timestamp)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_fields)
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
        var struc = new TStruct("Log");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "timestamp";
        field.Type = TType.I64;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(Timestamp, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "fields";
        field.Type = TType.List;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        {
          await oprot.WriteListBeginAsync(new TList(TType.Struct, Fields.Count), cancellationToken);
          foreach (Tag _iter3 in Fields)
          {
            await _iter3.WriteAsync(oprot, cancellationToken);
          }
          await oprot.WriteListEndAsync(cancellationToken);
        }
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
      var sb = new StringBuilder("Log(");
      sb.Append(", Timestamp: ");
      sb.Append(Timestamp);
      sb.Append(", Fields: ");
      sb.Append(Fields);
      sb.Append(")");
      return sb.ToString();
    }
  }

}
