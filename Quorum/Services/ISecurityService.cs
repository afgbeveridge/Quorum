using System.Collections.Generic;

namespace Quorum.Services {

    public interface ISecurityService {

        string EncodedFrameDirectivesFor(string host);

        string GetRawFrameDirectives();

        string DissectAndValidateFrame(string src);

        void ValidateRequestSize(long size);

        void ValidateDirectives(IDictionary<string, string> directives, object outerContext);

    }
}
