namespace WebProvider
{
    class Consts
    {
        public const string URL_REGEX =
            "(([a-zA-Z][0-9a-zA-Z+\\-\\.]*:)?/{0,2}[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?(#[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?";

        public const string COOKIE_PARSING_REGEX =
            @"(\s|\A)([^;=,]+)=([^;=,]+)(;|,)";
    }
}
