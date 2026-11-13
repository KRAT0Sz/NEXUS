namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests.Logic;

public sealed class CaiValidationTests
{
    [Test]
    public async Task Valid_128x128_PNG_Returns_True()
    {
        byte[] validPngBytes =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG Magic Header
            0x00, 0x00, 0x00, 0x0D,                         // IHDR Length (13)
            0x49, 0x48, 0x44, 0x52,                         // IHDR
            0x00, 0x00, 0x00, 0x80,                         // Width: 128
            0x00, 0x00, 0x00, 0x80                          // Height: 128
        ];

        bool isValid = CaiUploadController.IsValid128X128Png(validPngBytes, out string errorMessage);

        await Assert.That(isValid).IsTrue();
        await Assert.That(errorMessage).IsEmpty();
    }

    [Test]
    public async Task Non_PNG_Header_Returns_False_With_Error()
    {
        byte[] invalidHeaderBytes =
        [
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, // JPEG Magic Header
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x80,
            0x00, 0x00, 0x00, 0x80
        ];

        bool isValid = CaiUploadController.IsValid128X128Png(invalidHeaderBytes, out string errorMessage);

        await Assert.That(isValid).IsFalse();
        await Assert.That(errorMessage).Contains("PNG");
    }

    [Test]
    public async Task Truncated_Bytes_Returns_False()
    {
        byte[] shortBytes = [0x89, 0x50, 0x4E, 0x47];

        bool isValid = CaiUploadController.IsValid128X128Png(shortBytes, out string errorMessage);

        await Assert.That(isValid).IsFalse();
        await Assert.That(errorMessage).Contains("Not A Valid PNG Image");
    }

    [Test]
    public async Task Invalid_Dimensions_Returns_False()
    {
        byte[] wrongDimensionBytes =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x40,                         // Width: 64
            0x00, 0x00, 0x00, 0x40                          // Height: 64
        ];

        bool isValid = CaiUploadController.IsValid128X128Png(wrongDimensionBytes, out string errorMessage);

        await Assert.That(isValid).IsFalse();
        await Assert.That(errorMessage).Contains("128x128");
    }
}

