using System.Collections.Generic;
using ClassLibraryCommon;
using DCS_BIOS.Json;
using DCS_BIOS.Serialized;

namespace DCS_BIOS.misc
{
    public static class DCSBIOSArduinoInformation
    {
        public static List<string> GetInformation(DCSBIOSControl dcsbiosControl)
        {
            var result = new List<string>();
            if (dcsbiosControl == null) return result;

            if (dcsbiosControl.Inputs.Count > 0) { result.Add("Input"); }

            var fixedStepInputInfo = new List<string>();
            var integerOutputInfo = new List<string>();

            foreach (var input in dcsbiosControl.Inputs)
            {
                var interfaceType = new DCSBIOSInputInterface();
                interfaceType.Consume(dcsbiosControl.Identifier, input);
                switch (interfaceType.Interface)
                {
                    case DCSBIOSInputType.FIXED_STEP:
                        {
                            //RotaryEncoder_fixed_step
                            result.Add(RotaryEncoderFixedStep(dcsbiosControl, interfaceType));
                            break;
                        }
                    case DCSBIOSInputType.SET_STATE:
                        {
                            if (fixedStepInputInfo.Count == 0) fixedStepInputInfo.Add(CommonInputData(dcsbiosControl, interfaceType));

                            if (interfaceType.MaxValue < 33)
                            {
                                //SwitchMultiPos PIN0 Template
                                fixedStepInputInfo.Add(SwitchMultiPos(dcsbiosControl, interfaceType));
                            }

                            if (interfaceType.MaxValue == 1)
                            {
                                fixedStepInputInfo.Add(Switch2Pos(dcsbiosControl, interfaceType));
                                fixedStepInputInfo.Add(Matrix2Pos(dcsbiosControl, interfaceType));
                                //Switch2Pos
                                //Matrix2Pos
                            }

                            if (interfaceType.MaxValue == 2)
                            {
                                fixedStepInputInfo.Add(Switch3Pos(dcsbiosControl, interfaceType));
                                fixedStepInputInfo.Add(Matrix3Pos(dcsbiosControl, interfaceType));
                                //Switch3Pos
                                //Matrix3Pos
                            }

                            if (interfaceType.MaxValue < 20)
                            {
                                fixedStepInputInfo.Add(AnalogMultiPos(dcsbiosControl, interfaceType));
                                //AnalogMultiPos
                            }

                            if (interfaceType.MaxValue == DCSBIOSConstants.MAX_VALUE)
                            {
                                fixedStepInputInfo.Add(Potentiometer(dcsbiosControl, interfaceType));
                                //Potentiometer
                            }

                            break;
                        }
                    case DCSBIOSInputType.ACTION:
                        {
                            result.Add(ActionButton(dcsbiosControl, interfaceType));
                            //ActionButton
                            break;
                        }
                    case DCSBIOSInputType.VARIABLE_STEP:
                        {
                            result.Add(RotaryEncoderVariableStep(dcsbiosControl, interfaceType));
                            //RotaryEncoder_variable_step
                            break;
                        }
                    case DCSBIOSInputType.SET_STRING:
                    {
                        result.Add(StringInput(dcsbiosControl, interfaceType));
                        break;
                    }
                }
            }


            if (fixedStepInputInfo.Count > 0)
            {
                //Can have many so coalesce them with one banner. Banner is in [0].
                for (var i = 0; i < fixedStepInputInfo.Count; i++)
                {
                    if (i == 0) continue;
                    fixedStepInputInfo[0] += "\n\n" + fixedStepInputInfo[i];
                }

                result.Add(fixedStepInputInfo[0]);
            }

            if (dcsbiosControl.Outputs.Count > 0) { result.Add("Output"); }


            foreach (var output in dcsbiosControl.Outputs)
            {

                switch (output.OutputDataType)
                {
                    case DCSBiosOutputType.IntegerType:
                        {
                            if (integerOutputInfo.Count == 0) integerOutputInfo.Add(CommonOutputData(dcsbiosControl, output));

                            integerOutputInfo.Add(IntegerOutput(dcsbiosControl, output));

                            if (output.MaxValue == 1)
                            {
                                integerOutputInfo.Add(LEDOutput(dcsbiosControl, output, false));
                            }
                            if (output.MaxValue == DCSBIOSConstants.MAX_VALUE)
                            {
                                integerOutputInfo.Add(ServoOutput(dcsbiosControl, output));
                            }
                            break;
                        }
                    case DCSBiosOutputType.StringType:
                        {
                            result.Add(StringOutput(dcsbiosControl, output));
                            break;
                        }
                    case DCSBiosOutputType.LED:
                        {
                            result.Add(LEDOutput(dcsbiosControl, output, true));
                            break;
                        }
                    case DCSBiosOutputType.FloatBuffer: // does this still exist?
                        {
                            result.Add(FloatOutput(dcsbiosControl, output));
                            break;
                        }
                    case DCSBiosOutputType.ServoOutput:
                        {
                            result.Add(ServoOutput(dcsbiosControl, output));
                            break;
                        }
                }
            }

            if (integerOutputInfo.Count > 0)
            {
                //Can have many so coalesce them with one banner. Banner is in [0].
                for (var i = 0; i < integerOutputInfo.Count; i++)
                {
                    if (i == 0) continue;
                    integerOutputInfo[0] += "\n\n" + integerOutputInfo[i];
                }

                result.Add(integerOutputInfo[0]);
            }

            return result;

        }

        private static string MakeCamelCase(string str)
        {
            var returnValue = "";
            var capitalize = false;
            foreach (var c in str)
            {
                if (c == '_')
                {
                    capitalize = true;
                }
                else
                {
                    if (capitalize)
                    {
                        returnValue += char.ToUpperInvariant(c);
                        capitalize = false;
                    }
                    else
                    {
                        returnValue += char.ToLowerInvariant(c);
                    }
                }
            }

            return returnValue;
        }

        private static string RotaryEncoderFixedStep(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::RotaryEncoder '+idCamelCase(cid)+'("'+cid+'", "-'+io.suggested_step.toString()+'", "+'+io.suggested_step.toString()+'", '));
            var str = CommonInputData(dcsbiosControl, inputInterface);
            str += $"DcsBios::RotaryEncoder {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", \"DEC\", \"INC\", PIN_A, PIN_B);";
            return str;
        }

        private static string RotaryEncoderVariableStep(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::RotaryEncoder '+idCamelCase(cid)+'("'+cid+'", "-'+io.suggested_step.toString()+'", "+'+io.suggested_step.toString()+'", '));
            var str = CommonInputData(dcsbiosControl, inputInterface);
            str += $"DcsBios::RotaryEncoder {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", \"-{inputInterface.SuggestedStep}\", \"+{inputInterface.SuggestedStep}\", PIN_A, PIN_B);";
            return str;
        }

        private static string SwitchMultiPos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::SwitchMultiPos '+idCamelCase(cid)+'("'+cid+'", '+idCamelCase(cid+'_PINS')+', '+(io.max_value+1).toString()+');'));
            var pins = "";
            var valuePossibilities = inputInterface.MaxValue + 1;
            for (var i = 0; i < valuePossibilities; i++)
            {
                if (i > 0) pins += ", ";
                pins += $"PIN_{i}";
            }

            var str = $"const byte {MakeCamelCase(dcsbiosControl.Identifier)}Pins[{valuePossibilities}] = {{{pins}}};\n";
            str += $"DcsBios::SwitchMultiPos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", {MakeCamelCase(dcsbiosControl.Identifier)}Pins, {valuePossibilities});";
            return str;
        }

        private static string Switch2Pos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::Switch2Pos '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::Switch2Pos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", PIN);";
            return str;
        }

        private static string Switch3Pos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::Switch3Pos '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::Switch3Pos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", PIN_A, PIN_B);";
            return str;
        }

        private static string Matrix2Pos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::Matrix2Pos '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::Matrix2Pos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", ROW, COL);";
            return str;
        }

        private static string Matrix3Pos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::Matrix3Pos '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::Matrix3Pos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", ROW_A, COL_A, ROW_B, COL_B);";
            return str;
        }

        private static string AnalogMultiPos(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::AnalogMultiPos '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::AnalogMultiPos {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", PIN, STEPS);";
            return str;
        }

        private static string Potentiometer(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text('DcsBios::Potentiometer '+idCamelCase(cid)+'("'+cid+'", '));

            var str = $"DcsBios::Potentiometer {MakeCamelCase(dcsbiosControl.Identifier)}(\"{dcsbiosControl.Identifier}\", PIN);";
            return str;
        }

        private static string ActionButton(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            //code.append($("<span>").text("DcsBios::ActionButton "+idCamelCase(cid+"_"+io.argument)+'("'+cid+'", "'+io.argument+'", '));
            var str = CommonInputData(dcsbiosControl, inputInterface);
            str += $"DcsBios::ActionButton {MakeCamelCase(dcsbiosControl.Identifier + "_" + inputInterface.SpecifiedActionArgument)}(\"{dcsbiosControl.Identifier}\", \"{inputInterface.SpecifiedActionArgument}\", PIN);";
            return str;
        }

        private static string StringInput(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)
        {
            var functionName = MakeCamelCase(dcsbiosControl.Identifier) + "FreqSetFreq";
            var str = CommonInputData(dcsbiosControl, inputInterface);
            str += $"void {functionName}(\"{dcsbiosControl.Identifier}\", FREQUENCY, PIN);\n";
            return str;
        }

        private static string IntegerOutput(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output)
        {
            var functionName = MakeCamelCase("ON_" + dcsbiosControl.Identifier + "_CHANGE");

            //code.append($("<span>").text('void '+idCamelCase("ON_"+cid+"_CHANGE")+'(unsigned int newValue) {'));
            var str = $"void {functionName}(unsigned int newValue) \n{{\n";

            str += $"\t\t/* your code here */\n}}\n";

            //code.append($("<span>").text("DcsBios::IntegerBuffer "+idCamelCase(cid+"_BUFFER")+'('+io.address_identifier+', '+idCamelCase("ON_"+cid+"_CHANGE")+');'));
            //str += $"DcsBios::IntegerBuffer {MakeCamelCase(dcsbiosControl.Identifier + "_BUFFER")}({Common.GetHex(output.Address)}, {Common.GetHex(output.Mask)}, {output.ShiftBy}, {functionName});";
            str += $"DcsBios::IntegerBuffer {MakeCamelCase(dcsbiosControl.Identifier + "_BUFFER")}({output.AddressMaskShiftIdentifier}, {functionName});";
            return str;
        }

        private static string LEDOutput(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output, bool addBanner)
        {
            var functionName = MakeCamelCase(dcsbiosControl.Identifier);
            var str = "";
            if (addBanner) str += CommonOutputData(dcsbiosControl, output);
            //code.append($("<span>").text('DcsBios::LED '+idCamelCase(cid)+'('+io.address_identifier+', '));
            //str += $"DcsBios::LED {functionName}({Common.GetHex(output.Address)}, PIN);";
            str += $"DcsBios::LED {functionName}({output.AddressMaskIdentifier}, PIN);";

            return str;
        }

        private static string StringOutput(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output)
        {
            var functionName = MakeCamelCase("ON_" + dcsbiosControl.Identifier + "_CHANGE");
            var str = CommonOutputData(dcsbiosControl, output);
            //code.append($("<span>").text('void '+idCamelCase("ON_"+cid+"_CHANGE")+'(char* newValue) {'));
            str += $"void {functionName}((char* newValue)) \n{{\n";

            str += $"\t\t/* your code here */\n}}\n";

            //code.append($("<span>").text("DcsBios::StringBuffer<" + io.max_length.toString() + "> " + idCamelCase(cid + io.suffix + "_BUFFER") + '(' + io.address_identifier + ', ' + idCamelCase("ON_" + cid + "_CHANGE") + ');'));
            //str += $"DcsBios::StringBuffer<{output.MaxLength}> {MakeCamelCase(dcsbiosControl.Identifier + output.Suffix + "_BUFFER")}(\"{Common.GetHex(output.Address)}\", {functionName});";
            str += $"DcsBios::StringBuffer<{output.MaxLength}> {MakeCamelCase(dcsbiosControl.Identifier + output.Suffix + "_BUFFER")}({output.AddressIdentifier}, {functionName});";
            return str;
        }

        private static string ServoOutput(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output)
        {
            var functionName = MakeCamelCase(dcsbiosControl.Identifier);
            var str = CommonOutputData(dcsbiosControl, output);
            //code.append($("<span>").text('DcsBios::ServoOutput ' + idCamelCase(cid) + '(' + io.address_only_identifier + ', '));
            str += $"DcsBios::ServoOutput {functionName}({output.AddressIdentifier}, PIN, 544, 2400);";

            return str;
        }
        private static string FloatOutput(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output)
        {
            var functionName = MakeCamelCase(dcsbiosControl.Identifier + output.Suffix + "_BUFFER");
            var str = CommonOutputData(dcsbiosControl, output);
            //code.append($("<span>").text("DcsBios::FloatBuffer "+idCamelCase(cid+io.suffix+"_BUFFER")+'('+io.address_identifier+', '+io.value_range[0].toFixed()+', '+io.value_range[1]+ending+');'));
            //str += $"DcsBios::FloatBuffer {functionName}({Common.GetHex(output.Address)}, 0, {output.MaxLength});";
            str += $"DcsBios::FloatBuffer {functionName}({output.AddressMaskShiftIdentifier}, 0, {output.MaxLength});";

            return str;
        }

        private static string CommonInputData(DCSBIOSControl dcsbiosControl, DCSBIOSInputInterface inputInterface)//DCSBIOSInputInterface inputInterface)
        {
            var str = $"Interface :      {inputInterface.Interface}\n";
            switch (inputInterface.Interface)
            {
                case DCSBIOSInputType.ACTION:
                    {
                        str += $"Message :        {dcsbiosControl.Identifier} TOGGLE\n";
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        str += $"Message :        {dcsbiosControl.Identifier} <new_value>|-<decrease_by>|+<increase_by>\n";
                        str += $"Suggested Step : {inputInterface.SuggestedStep}\n";
                        str += $"Value Range :    0-{inputInterface.MaxValue}\n";
                        break;
                    }
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        str += $"Message :        {dcsbiosControl.Identifier} <DEC|INC>\n";
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        str += $"Message :        {dcsbiosControl.Identifier} <new_value>\n";
                        str += $"Value Range :    0-{inputInterface.MaxValue}\n";

                        break;
                    }
                case DCSBIOSInputType.SET_STRING:
                {
                    str += $"Message :        {dcsbiosControl.Identifier} set frequency\n";

                    break;
                }
            }

            str += $"Description :    {inputInterface.Description}\n";

            str += "\n";
            return str;
        }

        private static string CommonOutputData(DCSBIOSControl dcsbiosControl, DCSBIOSControlOutput output)
        {
            var str = "";
            if (output.OutputDataType != DCSBiosOutputType.StringType)
            {
                str = $"Type :        {output.Type}\n";
                str += $"Address :     {Common.GetHex(output.Address)}\n";
                str += $"Mask :        {Common.GetHex(output.Mask)}\n";
                str += $"ShiftBy :     {output.ShiftBy}\n";
                str += $"Max Value :   {output.MaxValue}\n";
                str += $"Description : {output.Description}\n";
                str += "\n";
            }
            else
            {
                str = $"Type :        {output.Type}\n";
                str += $"Address :     {Common.GetHex(output.Address)}\n";
                str += $"Max Length :  {output.MaxLength}\n";
                str += $"Description : {output.Description}\n";
                str += "\n";
            }
            return str;
        }
    }
}
