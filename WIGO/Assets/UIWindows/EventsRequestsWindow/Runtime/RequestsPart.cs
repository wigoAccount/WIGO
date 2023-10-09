using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class RequestsPart : AbstractPart
    {
        [SerializeField] OpeningVideoRequestElement _requestPrefab;
        [SerializeField] RectTransform _content;
        [SerializeField] GameObject _emptyRequestsContent;
        [SerializeField] GameObject _requestsContent;
        //[Space]
        //[SerializeField] EventCard[] _requestsData;
        Request[] _requestsData;

        List<OpeningVideoRequestElement> _requests = new List<OpeningVideoRequestElement>();

        public override void SetPartActive(bool active, bool animate = true)
        {
            if (active)
            {
                base.SetPartActive(active, animate);

                if (!_loaded)
                {
                    UpdateRequests();
                }
                else
                {
                    _requests.ForEach(x => x.Play());
                }
            }
            else
            {
                _requests.ForEach(x => x.ResetVideo());
                base.SetPartActive(active, animate);
            }
        }

        public override void ResetPart()
        {
            base.ResetPart();
            _requests.Clear();
            _content.DestroyChildren();
        }

        async void UpdateRequests()
        {
            _requestsContent.SetActive(false);
            _emptyRequestsContent.SetActive(false);
            _content.DestroyChildren();

            await Task.Delay(200);

            _loaded = true;
            if (_requestsData == null || _requestsData.Length == 0)
            {
                _requestsContent.SetActive(false);
                _emptyRequestsContent.SetActive(true);
                return;
            }

            _requestsContent.SetActive(true);
            _emptyRequestsContent.SetActive(false);
            foreach (var data in _requestsData)
            {
                var request = Instantiate(_requestPrefab, _content);
                request.Setup(data, OnManageRequest);
                _requests.Add(request);
            }
        }

        void OnManageRequest(OpeningVideoRequestElement request, bool accept)
        {

        }
    }
}
