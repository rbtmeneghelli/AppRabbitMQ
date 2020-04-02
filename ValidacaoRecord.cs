using System;

[Serializable]
public class ValidacaoRecord
{
    public Guid ObrigacaoItemId { get; set; }
    public Guid AtividadeItemId { get; set; }
    public Guid ClienteId { get; set; }
    public string TipoObrigacao { get; set; }
    public string NomeArquivo { get; set; }
    public byte[] ArquivoByte { get; set; }

    public ValidacaoRecord()
    {
        this.AtividadeItemId = new Guid();
        this.ObrigacaoItemId = new Guid();
        this.ClienteId = new Guid();
        this.TipoObrigacao = string.Empty;
        this.NomeArquivo = string.Empty;
        this.ArquivoByte = new byte[0];
    }
}
