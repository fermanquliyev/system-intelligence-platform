import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';

/** ABP may send these on outgoing requests; single-instance host ignores them — strip for clarity. */
const HEADERS_TO_OMIT = ['__tenant', 'Abp-Tenant-Id', 'abp-tenant-id'];

@Injectable()
export class ClearIsolationHeadersInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler,
  ): Observable<HttpEvent<unknown>> {
    let headers = req.headers;
    for (const name of HEADERS_TO_OMIT) {
      if (headers.has(name)) {
        headers = headers.delete(name);
      }
    }
    if (headers === req.headers) {
      return next.handle(req);
    }
    return next.handle(req.clone({ headers }));
  }
}
