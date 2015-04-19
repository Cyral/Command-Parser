package com.pyratron.frameworks.commands.parser;

import java.util.ArrayList;

/**
 * Represents an object that has multiple arguments/parameters.
 */
public interface IArguable // Calm down! Why are we arguing?!
{
    /**
     * The arguments the object contains. Arguments may be nested inside others to create links of arguments.
     */
    ArrayList<Argument> getArguments();
}
