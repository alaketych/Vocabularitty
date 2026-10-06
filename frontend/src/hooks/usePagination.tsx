import { useSearchParams } from "react-router-dom";
import {START_PAGE } from '../constants/index'

export default function usePagination() {
    const [params, setParams] = useSearchParams();

    const requestedPage = Number(params.get('pageNumber') || START_PAGE);

    const pageNumber = Number.isSafeInteger(requestedPage) 
        && requestedPage > 0 
        && requestedPage <= 178956970
        ? requestedPage : START_PAGE;
    
    function changePage(page: number) {
        setParams(previous => {
        const next = new URLSearchParams(previous);
        next.set('pageNumber', String(page));
        return next;
        });
    }

    return { 
        pageNumber, 
        changePage
    }
}