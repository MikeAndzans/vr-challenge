using vr_challenge.Data;
using vr_challenge.Errors;

namespace vr_challenge.Parser
{
    public enum LineType
    {
        HDR,
        LINE,
        UNKNOWN
    }

    public static class ShipmentParser
    {
        public const string HDR = "HDR";
        public const string LINE = "LINE";

        public const int HDR_PARAM_COUNT = 3;
        public const int LINE_PARAM_COUNT = 4;

        public static LineType GetLineType(string[] lineParams)
        {
            if (lineParams[0] == HDR)
            {
                if (lineParams.Length != HDR_PARAM_COUNT)
                    throw new ShipmentParserException($"Error while parsing HDR, incorrect token count: {lineParams.Length}, expected {HDR_PARAM_COUNT}!");

                return LineType.HDR;
            }

            if (lineParams[0] == LINE)
            {
                if (lineParams.Length != LINE_PARAM_COUNT)
                    throw new ShipmentParserException($"Error while parsing LINE, incorrect token count {lineParams.Length}, expected {LINE_PARAM_COUNT}!");

                return LineType.LINE;
            }

            throw new ShipmentParserException($"Error while parsing, unknown line identifier: {lineParams[0]}");
        }

        public static ShipmentBox ProcessHDRParams(string[] hdrParams)
        {
            return new ShipmentBox()
            {
                SupplierId = hdrParams[1],
                BoxId = hdrParams[2],
                BoxContents = new List<ProductShipment>()
            };
        }

        public static ProductShipment ProcessLineParams(string[] lineParams)
        {
            if (int.TryParse(lineParams[3], out int quantity))
            {
                return new ProductShipment()
                {
                    PoNumber = lineParams[1],
                    Barcode = lineParams[2],
                    Quantity = quantity
                };
            }

            throw new ShipmentParserException($"Error while parsing quantity value from LINE, expected int value, but found: {lineParams[3]}");            
        }
    }
}
