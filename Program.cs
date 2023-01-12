namespace jing.net;

using CatalogResolver = com.thaiopensource.resolver.catalog.CatalogResolver;
using Localizer = com.thaiopensource.util.Localizer;
using OptionParser = com.thaiopensource.util.OptionParser;
using PropertyMapBuilder = com.thaiopensource.util.PropertyMapBuilder;
using UriOrFile = com.thaiopensource.util.UriOrFile;
using Version = com.thaiopensource.util.Version;
using Flag = com.thaiopensource.validate.Flag;
using FlagOption = com.thaiopensource.validate.FlagOption;
using OptionArgumentException = com.thaiopensource.validate.OptionArgumentException;
using SchemaReader = com.thaiopensource.validate.SchemaReader;
using StringOption = com.thaiopensource.validate.StringOption;
using ValidateProperty = com.thaiopensource.validate.ValidateProperty;
using ValidationDriver = com.thaiopensource.validate.ValidationDriver;
using AutoSchemaReader = com.thaiopensource.validate.auto.AutoSchemaReader;
using RngProperty = com.thaiopensource.validate.prop.rng.RngProperty;
using CompactSchemaReader = com.thaiopensource.validate.rng.CompactSchemaReader;
using ErrorHandlerImpl = com.thaiopensource.xml.sax.ErrorHandlerImpl;
using InputSource = org.xml.sax.InputSource;
using SAXException = org.xml.sax.SAXException;

using IOException = java.io.IOException;
using JArrayList = java.util.ArrayList;
using JList = java.util.List;

public class Driver
{
    private static string usageKey = "usage";

    public static void setUsageKey(string key)
    {
        usageKey = key;
    }

    public static void Main(string[] args)
    {
        java.lang.System.exit(new Driver().DoMain(args));
    }

    private bool timing = false;
    private string encoding = null;
    private Localizer localizer = new Localizer((java.lang.Class)typeof(com.thaiopensource.relaxng.util.ValidationEngine));

    public int DoMain(string[] args)
    {
        ErrorHandlerImpl eh = new ErrorHandlerImpl(java.lang.System.@out);
        OptionParser op = new OptionParser("itcdfe:p:sC:", args);
        PropertyMapBuilder properties = new PropertyMapBuilder();
        properties.put(ValidateProperty.ERROR_HANDLER, eh);
        RngProperty.CHECK_ID_IDREF.add(properties);
        SchemaReader sr = null;
        bool compact = false;
        bool outputSimplifiedSchema = false;
        JList catalogUris = new JArrayList();

        try
        {
            while (op.moveToNextOption())
            {
                switch (op.getOptionChar())
                {
                    case 'i':
                        properties.put(RngProperty.CHECK_ID_IDREF, null);
                        break;
                    case 'C':
                        catalogUris.add(UriOrFile.toUri(op.getOptionArg()));
                        break;
                    case 'c':
                        compact = true;
                        break;
                    case 'd':
                        {
                            if (sr == null)
                                sr = new AutoSchemaReader();
                            FlagOption option = (FlagOption)sr.getOption(SchemaReader.BASE_URI + "diagnose");
                            if (option == null)
                            {
                                eh.print(localizer.message("no_schematron", op.getOptionCharString()));
                                return 2;
                            }
                            properties.put(option.getPropertyId(), Flag.PRESENT);
                        }
                        break;
                    case 't':
                        timing = true;
                        break;
                    case 'e':
                        encoding = op.getOptionArg();
                        break;
                    case 'f':
                        RngProperty.FEASIBLE.add(properties);
                        break;
                    case 's':
                        outputSimplifiedSchema = true;
                        break;
                    case 'p':
                        {
                            if (sr == null)
                                sr = new AutoSchemaReader();
                            StringOption option = (StringOption)sr.getOption(SchemaReader.BASE_URI + "phase");
                            if (option == null)
                            {
                                eh.print(localizer.message("no_schematron", op.getOptionCharString()));
                                return 2;
                            }
                            try
                            {
                                properties.put(option.getPropertyId(), option.valueOf(op.getOptionArg()));
                            }
                            catch (OptionArgumentException e)
                            {
                                eh.print(localizer.message("invalid_phase", op.getOptionArg()));
                                return 2;
                            }
                        }
                        break;
                }
            }
        }
        catch (OptionParser.InvalidOptionException e)
        {
            eh.print(localizer.message("invalid_option", op.getOptionCharString()));
            return 2;
        }
        catch (OptionParser.MissingArgumentException e)
        {
            eh.print(localizer.message("option_missing_argument", op.getOptionCharString()));
            return 2;
        }
        if (!catalogUris.isEmpty())
        {
            try
            {
                properties.put(ValidateProperty.RESOLVER, new CatalogResolver(catalogUris));
            }
            catch (java.lang.LinkageError e)
            {
                eh.print(localizer.message("resolver_not_found"));
                return 2;
            }
        }
        if (compact)
            sr = CompactSchemaReader.getInstance();
        args = op.getRemainingArgs();
        if (args.Length < 1)
        {
            eh.print(localizer.message(usageKey, Version.getVersion((java.lang.Class)typeof(com.thaiopensource.relaxng.util.ValidationEngine))));
            return 2;
        }
        long startTime = java.lang.System.currentTimeMillis();
        long loadedPatternTime = -1;
        bool hadError = false;
        try
        {
            ValidationDriver driver = new ValidationDriver(properties.toPropertyMap(), sr);
            InputSource @in = ValidationDriver.uriOrFileInputSource(args[0]);
            if (encoding != null)
                @in.setEncoding(encoding);
            if (driver.loadSchema(@in))
            {
                loadedPatternTime = java.lang.System.currentTimeMillis();
                if (outputSimplifiedSchema)
                {
                    string simplifiedSchema = (string)driver.getSchemaProperties().get(RngProperty.SIMPLIFIED_SCHEMA);
                    if (simplifiedSchema == null)
                    {
                        eh.print(localizer.message("no_simplified_schema"));
                        hadError = true;
                    }
                    else
                        java.lang.System.@out.print(simplifiedSchema);
                }
                for (int i = 1; i < args.Length; i++)
                {
                    if (!driver.validate(ValidationDriver.uriOrFileInputSource(args[i])))
                        hadError = true;
                }
            }
            else
                hadError = true;
        }
        catch (SAXException e)
        {
            hadError = true;
            eh.printException(e);
        }
        catch (IOException e)
        {
            hadError = true;
            eh.printException(e);
        }
        if (timing)
        {
            long endTime = java.lang.System.currentTimeMillis();
            if (loadedPatternTime < 0)
                loadedPatternTime = endTime;
            eh.print(localizer.message("elapsed_time",
                     new java.lang.Object[] {
                               new java.lang.Long(loadedPatternTime - startTime),
                               new java.lang.Long(endTime - loadedPatternTime),
                               new java.lang.Long(endTime - startTime)
                             }));
        }
        if (hadError)
            return 1;
        return 0;
    }
}
