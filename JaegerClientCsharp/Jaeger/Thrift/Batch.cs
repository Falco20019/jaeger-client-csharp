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

namespace Jaeger.Thrift
{

  public partial class Batch : TBase
  {

    public Process Process { get; set; }

    public List<Span> Spans { get; set; }

    public Batch()
    {
    }

    public Batch(Process process, List<Span> spans) : this()
    {
      this.Process = process;
      this.Spans = spans;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_process = false;
        bool isset_spans = false;
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
              if (field.Type == TType.Struct)
              {
                Process = new Process();
                await Process.ReadAsync(iprot, cancellationToken);
                isset_process = true;
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
                  Spans = new List<Span>();
                  TList _list20 = await iprot.ReadListBeginAsync(cancellationToken);
                  for(int _i21 = 0; _i21 < _list20.Count; ++_i21)
                  {
                    Span _elem22;
                    _elem22 = new Span();
                    await _elem22.ReadAsync(iprot, cancellationToken);
                    Spans.Add(_elem22);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
                isset_spans = true;
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
        if (!isset_process)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_spans)
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
        var struc = new TStruct("Batch");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "process";
        field.Type = TType.Struct;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await Process.WriteAsync(oprot, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "spans";
        field.Type = TType.List;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        {
          await oprot.WriteListBeginAsync(new TList(TType.Struct, Spans.Count), cancellationToken);
          foreach (Span _iter23 in Spans)
          {
            await _iter23.WriteAsync(oprot, cancellationToken);
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
      var sb = new StringBuilder("Batch(");
      sb.Append(", Process: ");
      sb.Append(Process== null ? "<null>" : Process.ToString());
      sb.Append(", Spans: ");
      sb.Append(Spans);
      sb.Append(")");
      return sb.ToString();
    }
  }

}
