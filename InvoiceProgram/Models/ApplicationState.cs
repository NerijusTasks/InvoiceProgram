namespace InvoiceProgram.Models
{
    public enum ApplicationState
    {
        ReadAmount,
        CheckIsPayingVat,
        ReadSupplierCountry,
        ReadClientCountry,
        CheckIsVatNeeded,
        ReadVatValue,
        CalculateTotalInvoice,
    }
}
